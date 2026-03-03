using Refit;

namespace McpSandbox.Mcp.Api;

internal static class ApiResponseExtensions
{
    internal static async Task<T> UnwrapAsync<T>(this Task<IApiResponse<T>> responseTask)
    {
        var response = await responseTask;
        return response.EnsureSuccess();
    }

    internal static T EnsureSuccess<T>(this IApiResponse<T> response)
    {
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(BuildErrorMessage(response));

        return response.Content!;
    }

    internal static void EnsureSuccess(this IApiResponse response)
    {
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(BuildErrorMessage(response));
    }

    private static string BuildErrorMessage(IApiResponse response)
    {
        var body = response.Error?.Content;
        return string.IsNullOrWhiteSpace(body)
            ? $"API call failed with status {(int)response.StatusCode} ({response.ReasonPhrase})."
            : $"API call failed with status {(int)response.StatusCode} ({response.ReasonPhrase}): {body}";
    }
}
