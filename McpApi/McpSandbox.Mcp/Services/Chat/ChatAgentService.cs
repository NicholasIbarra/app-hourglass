using System.ClientModel;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.EntityFrameworkCore;
using McpSandbox.Api.Contracts.Chat;
using McpSandbox.Mcp.Data;
using McpSandbox.Mcp.Domain.Entities;
using OpenAI.Chat;

namespace McpSandbox.Mcp.Services.Chat;

public sealed class ChatAgentService : IChatAgentService
{
    private readonly ChatDbContext _db;
    private readonly ChatClient _chatClient;
    private readonly ILogger<ChatAgentService> _logger;

    public ChatAgentService(ChatDbContext db, ChatClient chatClient, ILogger<ChatAgentService> logger)
    {
        _db = db;
        _chatClient = chatClient;
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

        var fullResponse = new StringBuilder();

        AsyncCollectionResult<StreamingChatCompletionUpdate> stream =
            _chatClient.CompleteChatStreamingAsync(messages, cancellationToken: cancellationToken);

        await foreach (var update in stream)
        {
            foreach (var part in update.ContentUpdate)
            {
                if (!string.IsNullOrEmpty(part.Text))
                {
                    fullResponse.Append(part.Text);
                    yield return part.Text;
                }
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
