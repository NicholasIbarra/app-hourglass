using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.EntityFrameworkCore;
using McpSandbox.Api.Contracts.Chat;
using McpSandbox.Mcp.Data;
using McpSandbox.Mcp.Domain.Entities;
using Microsoft.Extensions.Options;
using OpenAI.Chat;

namespace McpSandbox.Mcp.Services.Chat;

public sealed class ChatAgentService : IChatAgentService
{
    private readonly ChatDbContext _db;
    private readonly ChatClient _chatClient;
    private readonly IMcpToolClient _mcpToolClient;
    private readonly ILogger<ChatAgentService> _logger;
    private readonly ChatAgentOptions _options;

    public ChatAgentService(
        ChatDbContext db,
        ChatClient chatClient,
        IMcpToolClient mcpToolClient,
        IOptions<ChatAgentOptions> options,
        ILogger<ChatAgentService> logger)
    {
        _db = db;
        _chatClient = chatClient;
        _mcpToolClient = mcpToolClient;
        _options = options.Value;
        _logger = logger;
    }

    public async IAsyncEnumerable<string> StreamChatResponseAsync(
        Guid conversationId,
        string userMessage,
        Guid? promptVersionId,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var conversation = await _db.Conversations
            .Include(c => c.Messages.OrderBy(m => m.CreatedAt))
            .FirstOrDefaultAsync(c => c.Id == conversationId, cancellationToken)
            ?? throw new KeyNotFoundException($"Conversation {conversationId} not found.");

        var systemPrompt = await ResolveSystemPromptAsync(promptVersionId, cancellationToken);

        var userMsg = new ConversationMessage
        {
            ConversationId = conversationId,
            Role = MessageRole.User,
            Content = userMessage
        };
        _db.ConversationMessages.Add(userMsg);
        await _db.SaveChangesAsync(cancellationToken);

        var messages = new List<ChatMessage>();
        messages.Add(ChatMessage.CreateSystemMessage(systemPrompt));

        foreach (var msg in conversation.Messages)
        {
            messages.Add(msg.Role switch
            {
                MessageRole.System => ChatMessage.CreateSystemMessage(msg.Content),
                MessageRole.User => ChatMessage.CreateUserMessage(msg.Content),
                MessageRole.Assistant => ChatMessage.CreateAssistantMessage(msg.Content),
                _ => ChatMessage.CreateUserMessage(msg.Content)
            });
        }

        messages.Add(ChatMessage.CreateUserMessage(userMessage));

        var completionOptions = new ChatCompletionOptions();
        foreach (var tool in _mcpToolClient.GetToolDefinitions())
            completionOptions.Tools.Add(tool);

        var fullResponse = new StringBuilder();
        var invokedSignatures = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var maxIterations = Math.Clamp(_options.MaxToolIterations, 1, 32);

        for (var iteration = 0; iteration < maxIterations; iteration++)
        {
            var completion = await _chatClient.CompleteChatAsync(messages, completionOptions, cancellationToken);
            var update = completion.Value;

            if (update.ToolCalls.Count > 0)
            {
                foreach (var toolCall in update.ToolCalls)
                {
                    var signature = $"{toolCall.FunctionName}:{toolCall.FunctionArguments}";
                    if (!invokedSignatures.Add(signature))
                    {
                        _logger.LogWarning(
                            "Detected repeated tool call loop for {ToolName}. Breaking loop to avoid repeated confirmations.",
                            toolCall.FunctionName);

                        var loopMessage = "I’m pausing because I’m repeating the same action request. " +
                                          "Please provide any missing details (for example an office id, date range, or exact action) and I’ll proceed.";
                        fullResponse.Append(loopMessage);
                        yield return loopMessage;
                        break;
                    }

                    try
                    {
                        var toolResult = await _mcpToolClient.InvokeAsync(
                            toolCall.FunctionName,
                            toolCall.FunctionArguments.ToString(),
                            cancellationToken);

                        messages.Add(ChatMessage.CreateSystemMessage(
                            $"Tool '{toolCall.FunctionName}' executed successfully. Result: {toolResult}. " +
                            "If the user already confirmed this action, do not ask for confirmation again; continue with the next step or summarize the completed action."));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "MCP tool {ToolName} failed.", toolCall.FunctionName);
                        messages.Add(ChatMessage.CreateSystemMessage(
                            $"Tool '{toolCall.FunctionName}' failed: {ex.Message}. Ask a concise follow-up question only if needed to proceed."));
                    }
                }

                if (fullResponse.Length > 0)
                    break;

                continue;
            }

            foreach (var part in update.Content)
            {
                if (string.IsNullOrEmpty(part.Text))
                    continue;

                fullResponse.Append(part.Text);
                yield return part.Text;
            }

            break;
        }

        if (fullResponse.Length == 0)
        {
            const string fallbackMessage = "I couldn't produce a response. Please try again with a bit more detail.";
            fullResponse.Append(fallbackMessage);
            yield return fallbackMessage;
        }

        var assistantMsg = new ConversationMessage
        {
            ConversationId = conversationId,
            Role = MessageRole.Assistant,
            Content = fullResponse.ToString()
        };
        _db.ConversationMessages.Add(assistantMsg);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task<string> ResolveSystemPromptAsync(Guid? promptVersionId, CancellationToken cancellationToken)
    {
        if (promptVersionId.HasValue)
        {
            var version = await _db.PromptVersions
                .FirstOrDefaultAsync(v => v.Id == promptVersionId.Value, cancellationToken);

            if (version is not null)
                return version.Content;
        }

        var published = await _db.PromptVersions
            .Include(v => v.Prompt)
            .Where(v => v.Prompt.Name == "scheduling-assistant" && v.IsPublished)
            .OrderByDescending(v => v.VersionNumber)
            .FirstOrDefaultAsync(cancellationToken);

        if (published is not null)
            return published.Content;

        return DefaultSystemPrompt.FallbackPrompt;
    }
}
