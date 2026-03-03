using Newtonsoft.Json;

namespace ChatConsole.Models;

public sealed class ExtractionResponse
{
    [JsonProperty("status")]
    public string? Status { get; set; }

    [JsonProperty("data")]
    public ExtractionData? Data { get; set; }

    [JsonProperty("missingFields")]
    public List<string>? MissingFields { get; set; }

    [JsonProperty("followUpPrompt")]
    public string? FollowUpPrompt { get; set; }
}

public sealed class ExtractionData
{
    [JsonProperty("FirstName")]
    public string? FirstName { get; set; }

    [JsonProperty("LastName")]
    public string? LastName { get; set; }

    [JsonProperty("DateOfBirth")]
    public string? DateOfBirth { get; set; }

    [JsonProperty("Email")]
    public string? Email { get; set; }

    [JsonProperty("Notes")]
    public string? Notes { get; set; }
}
