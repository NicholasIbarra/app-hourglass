using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using McpSandbox.Mcp.Tools;
using ModelContextProtocol.Server;
using OpenAI.Chat;

namespace McpSandbox.Mcp.Services.Chat;

public sealed class McpToolClient : IMcpToolClient
{
    private const int MaxSchemaDepth = 3;

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
        var parameters = method.GetParameters();
        var args = new object?[parameters.Length];

        var singleToolParameter = parameters
            .Where(p => p.ParameterType != typeof(CancellationToken))
            .ToArray();

        var canUseRootObjectForSingleComplexParameter =
            argsDoc is not null &&
            argsDoc.RootElement.ValueKind == JsonValueKind.Object &&
            singleToolParameter.Length == 1 &&
            IsComplexType(singleToolParameter[0].ParameterType);

        for (var i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];

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

            if (canUseRootObjectForSingleComplexParameter)
            {
                args[i] = argsDoc!.RootElement.Deserialize(parameter.ParameterType, JsonOptions);
                continue;
            }

            if (parameter.HasDefaultValue)
            {
                args[i] = parameter.DefaultValue;
                continue;
            }

            if (IsRequiredParameter(parameter))
                throw new ArgumentException($"Missing required argument '{parameter.Name}'.");

            args[i] = null;
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

    private static bool IsComplexType(Type type)
    {
        var unwrapped = Nullable.GetUnderlyingType(type) ?? type;
        if (unwrapped == typeof(string))
            return false;

        if (unwrapped.IsPrimitive || unwrapped.IsEnum)
            return false;

        return true;
    }

    private static bool IsRequiredParameter(ParameterInfo parameter)
    {
        if (parameter.HasDefaultValue)
            return false;

        var type = parameter.ParameterType;

        if (type.IsValueType)
            return Nullable.GetUnderlyingType(type) is null;

        var nullability = new NullabilityInfoContext().Create(parameter);
        return nullability.WriteState == NullabilityState.NotNull;
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
                if (method.IsSpecialName)
                    continue;

                var toolAttribute = method.GetCustomAttribute<McpServerToolAttribute>(inherit: true);

                var name = string.IsNullOrWhiteSpace(toolAttribute?.Name)
                    ? method.Name
                    : toolAttribute.Name;

                var description = method.GetCustomAttribute<DescriptionAttribute>(inherit: true)?.Description
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

            var schema = BuildTypeSchema(parameter.ParameterType, 0);

            var description = parameter.GetCustomAttribute<DescriptionAttribute>()?.Description;
            if (!string.IsNullOrWhiteSpace(description))
                schema["description"] = description;

            properties[parameter.Name!] = schema;

            if (IsRequiredParameter(parameter))
                required.Add(parameter.Name!);
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

    private static Dictionary<string, object?> BuildTypeSchema(Type type, int depth)
    {
        var unwrapped = Nullable.GetUnderlyingType(type) ?? type;

        if (TryGetPrimitiveJsonType(unwrapped, out var primitiveType))
            return new Dictionary<string, object?> { ["type"] = primitiveType };

        if (TryGetEnumerableElementType(unwrapped, out var elementType))
        {
            return new Dictionary<string, object?>
            {
                ["type"] = "array",
                ["items"] = BuildTypeSchema(elementType, depth + 1)
            };
        }

        if (depth >= MaxSchemaDepth)
            return new Dictionary<string, object?> { ["type"] = "object" };

        var objectProperties = new Dictionary<string, object?>();
        var objectRequired = new List<string>();

        var nullabilityContext = new NullabilityInfoContext();
        foreach (var property in unwrapped.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!property.CanRead)
                continue;

            var propertySchema = BuildTypeSchema(property.PropertyType, depth + 1);
            var description = property.GetCustomAttribute<DescriptionAttribute>()?.Description;
            if (!string.IsNullOrWhiteSpace(description))
                propertySchema["description"] = description;

            objectProperties[property.Name] = propertySchema;

            var nullabilityInfo = nullabilityContext.Create(property);
            var isNullableRef = !property.PropertyType.IsValueType && nullabilityInfo.ReadState != NullabilityState.NotNull;
            var isNullableValueType = Nullable.GetUnderlyingType(property.PropertyType) is not null;
            if (!isNullableRef && !isNullableValueType)
                objectRequired.Add(property.Name);
        }

        return new Dictionary<string, object?>
        {
            ["type"] = "object",
            ["properties"] = objectProperties,
            ["required"] = objectRequired,
            ["additionalProperties"] = false
        };
    }

    private static bool TryGetPrimitiveJsonType(Type type, out string jsonType)
    {
        if (type == typeof(bool))
        {
            jsonType = "boolean";
            return true;
        }

        if (type == typeof(int) || type == typeof(long) || type == typeof(short))
        {
            jsonType = "integer";
            return true;
        }

        if (type == typeof(float) || type == typeof(double) || type == typeof(decimal))
        {
            jsonType = "number";
            return true;
        }

        if (type == typeof(string) || type == typeof(Guid) || type == typeof(DateTime) || type == typeof(DateTimeOffset) || type == typeof(DateOnly))
        {
            jsonType = "string";
            return true;
        }

        if (type.IsEnum)
        {
            jsonType = "string";
            return true;
        }

        jsonType = string.Empty;
        return false;
    }

    private static bool TryGetEnumerableElementType(Type type, out Type elementType)
    {
        if (type == typeof(string))
        {
            elementType = typeof(object);
            return false;
        }

        if (type.IsArray)
        {
            elementType = type.GetElementType()!;
            return true;
        }

        if (type.IsGenericType)
        {
            var generic = type.GetGenericTypeDefinition();
            if (generic == typeof(IEnumerable<>) ||
                generic == typeof(ICollection<>) ||
                generic == typeof(IList<>) ||
                generic == typeof(List<>) ||
                generic == typeof(IReadOnlyList<>))
            {
                elementType = type.GetGenericArguments()[0];
                return true;
            }
        }

        var enumerableInterface = type.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

        if (enumerableInterface is not null)
        {
            elementType = enumerableInterface.GetGenericArguments()[0];
            return true;
        }

        if (typeof(IEnumerable).IsAssignableFrom(type))
        {
            elementType = typeof(object);
            return true;
        }

        elementType = typeof(object);
        return false;
    }

    private sealed record ToolRegistration(
        string Name,
        Type DeclaringType,
        MethodInfo Method,
        ChatTool Definition);
}
