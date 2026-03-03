using System.ComponentModel;

namespace McpSandbox.Api.Contracts.Schedule;

public sealed record CancelShiftRequestRequest(
    [property: Description("Optional notes explaining the cancellation.")] string? Notes);
