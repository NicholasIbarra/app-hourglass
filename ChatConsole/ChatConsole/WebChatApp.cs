using Microsoft.Extensions.AI;
using Microsoft.Extensions.Hosting;
using ChatConsole.Models;
using Newtonsoft.Json;

namespace ChatConsole;

public partial class WebChatApp(HttpClient httpClient, IChatClient ai, IHostApplicationLifetime lifetime) : BackgroundService
{
    private List<ChatMessage> history = [];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("Starting WebChatApp...");

        // Seed the conversation once so we keep a stable system instruction across turns.
        // Re-adding it every loop duplicates instructions and can degrade output quality.
        ResetHistory();

        // Simulate some work
        while (!stoppingToken.IsCancellationRequested)
        {
            Console.WriteLine("WebChatApp is running...");

            Console.Write("Prompt: ");
            string? userInput = Console.ReadLine();

            // If the user enters an incomplete/empty message, don't end the session.
            // Just avoid adding a new user turn and keep the existing history.
            if (!string.IsNullOrWhiteSpace(userInput))
            {
                history.Add(new ChatMessage(ChatRole.User, userInput));
            }

            ChatResponse response = await ai.GetResponseAsync(history, cancellationToken: stoppingToken);

            history.AddMessages(response);

            foreach (var msg in response.Messages)
            {
                Console.WriteLine($"{msg.Role}: {msg.Text}");
            }

            // If we extracted a complete/valid form, clear prior chat context so the next
            // request starts fresh.
            if (IsCompleteExtraction(response))
            {
                Console.WriteLine("(Form complete — starting a new session.)");
                ResetHistory();
            }
        }

        Console.WriteLine("WebChatApp is stopping...");
    }

    private void ResetHistory()
    {
        history.Clear();
        history.Add(new ChatMessage(ChatRole.System, prompt));
    }

    private static bool IsCompleteExtraction(ChatResponse response)
    {
        // The prompt instructs the assistant to return ONLY JSON.
        // Consider it complete when status == "complete".
        foreach (var msg in response.Messages)
        {
            if (msg.Role != ChatRole.Assistant)
            {
                continue;
            }

            var text = msg.Text;
            if (string.IsNullOrWhiteSpace(text))
            {
                continue;
            }

            try
            {
                var parsed = JsonConvert.DeserializeObject<ExtractionResponse>(text);
                if (string.Equals(parsed?.Status, "complete", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            catch (JsonException)
            {
                // If the model returned non-JSON, just don't reset.
            }
        }

        return false;
    }
}
