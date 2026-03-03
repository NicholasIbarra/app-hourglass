
using Azure.AI.OpenAI;
using ChatConsole;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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

host.Services.AddChatClient(chatClient);
host.Services.AddHttpClient();

//host.Services.AddHostedService<ChatApp>();
host.Services.AddHostedService<WebChatApp>();


var app = host.Build();

app.Run();