using Microsoft.Playwright;
using System.Text.Json;

namespace Restaurant_Aqa_Framework.Api;

public static class ApiResponseExtensions
{
    public static async Task<T> ReadAsAsync<T>(this IAPIResponse response)
    {
        var json = await response.TextAsync();
        return JsonSerializer.Deserialize<T>(json, JsonDefaults.Options)
            ?? throw new InvalidOperationException(
                $"Response body deserialized to null for {typeof(T).Name}. Status {response.Status}, body: {json}");
    }

    /// <summary>
    /// The backend's error shape is <c>{"error": "..."}</c> (ResponseBuilder.CreateErrorResponse) for
    /// almost every resource, but a few handlers (e.g. POST /tables) return a plain-text 400 body instead
    /// of JSON. This falls back to the raw body so callers never throw while reading an error message.
    /// </summary>
    public static async Task<string> ReadErrorMessageAsync(this IAPIResponse response)
    {
        var json = await response.TextAsync();
        try
        {
            var parsed = JsonSerializer.Deserialize<JsonElement>(json);
            if (parsed.ValueKind == JsonValueKind.Object && parsed.TryGetProperty("error", out var error))
                return error.GetString() ?? json;
        }
        catch (JsonException)
        {
            // Not JSON — plain-text error body (e.g. POST /tables validation failures).
        }

        return json;
    }
}
