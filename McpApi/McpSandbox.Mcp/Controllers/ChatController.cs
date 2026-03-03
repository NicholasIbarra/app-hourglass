using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using McpSandbox.Api.Contracts;
using McpSandbox.Api.Contracts.Chat;
using McpSandbox.Mcp.Data;
using McpSandbox.Mcp.Domain.Entities;
using McpSandbox.Mcp.Services.Chat;

namespace McpSandbox.Mcp.Controllers;

[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly ChatDbContext _db;
    private readonly IChatAgentService _chatAgent;

    public ChatController(ChatDbContext db, IChatAgentService chatAgent)
    {
        _db = db;
        _chatAgent = chatAgent;
    }

    [HttpPost("conversations")]
    public async Task<ActionResult<ConversationDto>> CreateConversation(
        [FromBody] CreateConversationRequest request,
        CancellationToken cancellationToken)
    {
        var conversation = new Conversation
        {
            Title = request.Title,
            UserId = request.UserId,
            Status = ConversationStatus.Active
        };

        _db.Conversations.Add(conversation);

        if (!string.IsNullOrWhiteSpace(request.InitialMessage))
        {
            var message = new ConversationMessage
            {
                ConversationId = conversation.Id,
                Role = MessageRole.User,
                Content = request.InitialMessage
            };
            _db.ConversationMessages.Add(message);
        }

        await _db.SaveChangesAsync(cancellationToken);

        var dto = await GetConversationDtoAsync(conversation.Id, cancellationToken);
        return CreatedAtAction(nameof(GetConversation), new { id = conversation.Id }, dto);
    }

    [HttpPost("conversations/{id:guid}/messages")]
    public async Task SendMessage(
        Guid id,
        [FromBody] SendMessageRequest request,
        CancellationToken cancellationToken)
    {
        Response.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache";
        Response.Headers.Connection = "keep-alive";

        await foreach (var chunk in _chatAgent.StreamChatResponseAsync(
            id, request.Content, request.PromptVersionId, cancellationToken))
        {
            await Response.WriteAsync($"data: {chunk}\n\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }

        await Response.WriteAsync("data: [DONE]\n\n", cancellationToken);
        await Response.Body.FlushAsync(cancellationToken);
    }

    [HttpGet("conversations/{id:guid}")]
    public async Task<ActionResult<ConversationDto>> GetConversation(
        Guid id,
        CancellationToken cancellationToken)
    {
        var dto = await GetConversationDtoAsync(id, cancellationToken);

        if (dto is null)
            return NotFound();

        return Ok(dto);
    }

    [HttpGet("conversations")]
    public async Task<ActionResult<PagedResult<ConversationSummaryDto>>> ListConversations(
        [FromQuery] Guid? userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Conversations.AsNoTracking().AsQueryable();

        if (userId.HasValue)
            query = query.Where(c => c.UserId == userId.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new ConversationSummaryDto(
                c.Id,
                c.Title,
                c.UserId,
                c.Status,
                c.CreatedAt,
                c.Messages.Count))
            .ToListAsync(cancellationToken);

        return Ok(new PagedResult<ConversationSummaryDto>(page, pageSize, totalCount, items));
    }

    private async Task<ConversationDto?> GetConversationDtoAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _db.Conversations
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new ConversationDto(
                c.Id,
                c.Title,
                c.UserId,
                c.Status,
                c.CreatedAt,
                c.UpdatedAt,
                c.Messages
                    .OrderBy(m => m.CreatedAt)
                    .Select(m => new ConversationMessageDto(m.Id, m.Role, m.Content, m.CreatedAt))
                    .ToList()))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
