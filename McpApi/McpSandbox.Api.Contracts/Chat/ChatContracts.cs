namespace McpSandbox.Api.Contracts.Chat;

public sealed record CreateConversationRequest(
    string? Title,
    Guid? UserId,
    string? InitialMessage);

public sealed record SendMessageRequest(
    string Content,
    Guid? PromptVersionId);

public sealed record ConversationDto(
    Guid Id,
    string? Title,
    Guid? UserId,
    ConversationStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    IReadOnlyList<ConversationMessageDto> Messages);

public sealed record ConversationSummaryDto(
    Guid Id,
    string? Title,
    Guid? UserId,
    ConversationStatus Status,
    DateTimeOffset CreatedAt,
    int MessageCount);

public sealed record ConversationMessageDto(
    Guid Id,
    MessageRole Role,
    string Content,
    DateTimeOffset CreatedAt);
