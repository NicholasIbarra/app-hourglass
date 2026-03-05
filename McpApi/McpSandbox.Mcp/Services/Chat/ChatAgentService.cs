using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using McpSandbox.Api.Contracts.Chat;
using McpSandbox.Mcp.Data;
using McpSandbox.Mcp.Domain.Entities;

namespace McpSandbox.Mcp.Services.Chat;

public sealed class ChatAgentService : IChatAgentService
{
    private readonly ChatDbContext _db;
    private readonly IChatClient _chatClient;
    private readonly IMcpToolProvider _toolProvider;
    private readonly ILogger<ChatAgentService> _logger;

    public ChatAgentService(
        ChatDbContext db,
        IChatClient chatClient,
        IMcpToolProvider toolProvider,
        ILogger<ChatAgentService> logger)
    {
        _db = db;
        _chatClient = chatClient;
        _toolProvider = toolProvider;
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
        messages.Add(new ChatMessage(ChatRole.System, systemPrompt));

        foreach (var msg in conversation.Messages)
        {
            var role = msg.Role switch
            {
                MessageRole.System => ChatRole.System,
                MessageRole.User => ChatRole.User,
                MessageRole.Assistant => ChatRole.Assistant,
                _ => ChatRole.User
            };
            messages.Add(new ChatMessage(role, msg.Content));
        }

        messages.Add(new ChatMessage(ChatRole.User, userMessage));

        var tools = await _toolProvider.GetToolsAsync(cancellationToken);
        var chatOptions = new ChatOptions();
        if (tools.Count > 0)
        {
            chatOptions.Tools = [.. tools];
        }

        var fullResponse = new StringBuilder();

        await foreach (var update in _chatClient.GetStreamingResponseAsync(
            messages, chatOptions, cancellationToken))
        {
            if (!string.IsNullOrEmpty(update.Text))
            {
                fullResponse.Append(update.Text);
                yield return update.Text;
            }
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
