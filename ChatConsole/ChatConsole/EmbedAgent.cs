using Microsoft.Extensions.AI;
using Microsoft.Extensions.Hosting;

namespace ChatConsole;

public partial class EmbedAgent(IChatClient ai, IHostApplicationLifetime lifetime) : BackgroundService
{
    private List<ChatMessage> history = [];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("Starting EmbedAgent...");

        ResetHistory();

        while (!stoppingToken.IsCancellationRequested)
        {
            Console.Write("Prompt: ");
            string? userInput = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(userInput))
                continue;

            history.Add(new ChatMessage(ChatRole.User, userInput));

            ChatResponse response = await ai.GetResponseAsync(history, cancellationToken: stoppingToken);

            history.AddMessages(response);

            foreach (var msg in response.Messages)
            {
                Console.WriteLine($"{msg.Role}: {msg.Text}");
            }
        }

        Console.WriteLine("EmbedAgent is stopping...");
    }

    private void ResetHistory()
    {
        history.Clear();
        history.Add(new ChatMessage(ChatRole.System, prompt));
    }
}
