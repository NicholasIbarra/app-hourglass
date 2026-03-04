using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using McpSandbox.Mcp.Tools;
using ModelContextProtocol.Server;
using OpenAI.Chat;

namespace McpSandbox.Mcp.Services.Chat;

public sealed class McpToolClient : IMcpToolClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<string, ToolRegistration> _tools;
    private readonly IReadOnlyList<ChatTool> _toolDefinitions;

    public McpToolClient(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;

        _tools = DiscoverTools().ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
        _toolDefinitions = _tools.Values
            .Select(x => x.Definition)
            .ToList();
    }

    public IReadOnlyList<ChatTool> GetToolDefinitions() => _toolDefinitions;

    public async Task<string> InvokeAsync(string toolName, string? toolArgumentsJson, CancellationToken cancellationToken = default)
    {
        if (!_tools.TryGetValue(toolName, out var registration))
            throw new KeyNotFoundException($"Unknown MCP tool '{toolName}'.");

        var instance = ActivatorUtilities.GetServiceOrCreateInstance(_serviceProvider, registration.DeclaringType);
        var argsDoc = string.IsNullOrWhiteSpace(toolArgumentsJson)
            ? null
            : JsonDocument.Parse(toolArgumentsJson);

        var invocationArgs = BuildInvocationArguments(registration.Method, argsDoc, cancellationToken);
        var result = registration.Method.Invoke(instance, invocationArgs);

        return await SerializeResultAsync(result, cancellationToken);
    }

    private static object?[] BuildInvocationArguments(MethodInfo method, JsonDocument? argsDoc, CancellationToken cancellationToken)
    {
        var args = new object?[method.GetParameters().Length];

        for (var i = 0; i < method.GetParameters().Length; i++)
        {
            var parameter = method.GetParameters()[i];

            if (parameter.ParameterType == typeof(CancellationToken))
            {
                args[i] = cancellationToken;
                continue;
            }

            if (argsDoc is not null &&
                argsDoc.RootElement.ValueKind == JsonValueKind.Object &&
                TryGetPropertyIgnoreCase(argsDoc.RootElement, parameter.Name!, out var property))
            {
                args[i] = property.Deserialize(parameter.ParameterType, JsonOptions);
                continue;
            }

            if (parameter.HasDefaultValue)
            {
                args[i] = parameter.DefaultValue;
                continue;
            }

            if (!parameter.ParameterType.IsValueType || Nullable.GetUnderlyingType(parameter.ParameterType) is not null)
            {
                args[i] = null;
                continue;
            }

            throw new ArgumentException($"Missing required argument '{parameter.Name}'.");
        }

        return args;
    }

    private static bool TryGetPropertyIgnoreCase(JsonElement element, string name, out JsonElement value)
    {
        foreach (var property in element.EnumerateObject())
        {
            if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    private static async Task<string> SerializeResultAsync(object? result, CancellationToken cancellationToken)
    {
        if (result is null)
            return "";

        if (result is Task task)
        {
            await task.WaitAsync(cancellationToken);

            var taskType = task.GetType();
            if (taskType.IsGenericType)
            {
                var taskResult = taskType.GetProperty("Result")?.GetValue(task);
                return SerializeToJson(taskResult);
            }

            return "";
        }

        return SerializeToJson(result);
    }

    private static string SerializeToJson(object? value)
        => value switch
        {
            null => "",
            string s => s,
            _ => JsonSerializer.Serialize(value, JsonOptions)
        };

    private static IEnumerable<ToolRegistration> DiscoverTools()
    {
        var toolTypes = new[]
        {
            typeof(UserMcpTools),
            typeof(OfficeMcpTools),
            typeof(AvailabilityMcpTools),
            typeof(UnavailabilityMcpTools),
            typeof(ScheduleMcpTools)
        };

        foreach (var toolType in toolTypes)
        {
            foreach (var method in toolType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                var toolAttribute = method.GetCustomAttribute<McpServerToolAttribute>();
                if (toolAttribute is null)
                    continue;

                var name = string.IsNullOrWhiteSpace(toolAttribute.Name)
                    ? method.Name
                    : toolAttribute.Name;

                var description = method.GetCustomAttribute<DescriptionAttribute>()?.Description
                    ?? $"Invokes {name}.";

                yield return new ToolRegistration(
                    name,
                    toolType,
                    method,
                    ChatTool.CreateFunctionTool(name, description, BuildSchema(method)));
            }
        }
    }

    private static BinaryData BuildSchema(MethodInfo method)
    {
        var properties = new Dictionary<string, object?>();
        var required = new List<string>();

        foreach (var parameter in method.GetParameters())
        {
            if (parameter.ParameterType == typeof(CancellationToken))
                continue;

            var schema = new Dictionary<string, object?>
            {
                ["type"] = GetJsonType(parameter.ParameterType)
            };

            var description = parameter.GetCustomAttribute<DescriptionAttribute>()?.Description;
            if (!string.IsNullOrWhiteSpace(description))
                schema["description"] = description;

            properties[parameter.Name!] = schema;

            if (!parameter.HasDefaultValue &&
                parameter.ParameterType.IsValueType &&
                Nullable.GetUnderlyingType(parameter.ParameterType) is null)
            {
                required.Add(parameter.Name!);
            }
        }

        var payload = new Dictionary<string, object?>
        {
            ["type"] = "object",
            ["properties"] = properties,
            ["required"] = required,
            ["additionalProperties"] = false
        };

        return BinaryData.FromObjectAsJson(payload, JsonOptions);
    }

    private static string GetJsonType(Type parameterType)
    {
        var type = Nullable.GetUnderlyingType(parameterType) ?? parameterType;

        if (type == typeof(bool)) return "boolean";
        if (type == typeof(int) || type == typeof(long) || type == typeof(short)) return "integer";
        if (type == typeof(float) || type == typeof(double) || type == typeof(decimal)) return "number";
        if (type == typeof(string) || type == typeof(Guid) || type == typeof(DateTime) || type == typeof(DateTimeOffset) || type == typeof(DateOnly)) return "string";
        if (typeof(System.Collections.IEnumerable).IsAssignableFrom(type) && type != typeof(string)) return "array";

        return "object";
    }

    private sealed record ToolRegistration(
        string Name,
        Type DeclaringType,
        MethodInfo Method,
        ChatTool Definition);
}
