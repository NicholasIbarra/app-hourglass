using Azure.AI.OpenAI;
using ChatConsole;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenAI.Chat;

var host = Host.CreateApplicationBuilder(args);

// add user secrets
host.Configuration.AddUserSecrets<Program>();

var endpoint = host.Configuration["Chat:AzureOpenAI:Endpoint"]
    ?? throw new InvalidOperationException("Azure OpenAI endpoint is not configured.");

var key = host.Configuration["Chat:AzureOpenAI:Key"]
    ?? throw new InvalidOperationException("Azure OpenAI key is not configured.");

var client = new AzureOpenAIClient(new Uri(endpoint), new Azure.AzureKeyCredential(key));

string model = "gpt-5-chat";
IChatClient chatClient = client.GetChatClient(model).AsIChatClient();

var tool = ChatTool.CreateFunctionTool(
    functionName: "mcp_call",
    functionDescription: "Calls the MCP server to execute a domain operation.",
    functionParameters: BinaryData.FromObjectAsJson(new
    {
        type = "object",
        properties = new
        {
            method = new { type = "string" },
            parameters = new { type = "object" }
        },
        required = new[] { "method", "parameters" }
    })
);

host.Services.AddChatClient(chatClient);
host.Services.AddHttpClient();

//host.Services.AddHostedService<ChatApp>();
host.Services.AddHostedService<EmbedAgent>();


var app = host.Build();

app.Run();app.Run();