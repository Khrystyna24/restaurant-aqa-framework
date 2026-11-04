using Microsoft.Playwright;
using Restaurant_Aqa_Framework.Configuration;
using System.Text.Json;

namespace Restaurant_Aqa_Framework.Api;

/// <summary>
/// Base for per-resource API clients. Always builds a fully-qualified URL from
/// <see cref="Settings.ApiBaseUrl"/> rather than trusting the request context's own base URL —
/// a `@ui` scenario's <c>ICallTheApi</c> ability is backed by the browser context's
/// <c>APIRequest</c>, whose base URL is the UI origin, not the API origin.
/// </summary>
public abstract class ApiClientBase(IAPIRequestContext api)
{
    private const int MaxAttempts = 3;

    protected IAPIRequestContext Api { get; } = api;

    // The shared "develop" backend answers an occasional transient 500 ({"message": "Internal server
    // error"} from API Gateway/Lambda). Idempotent calls are retried a couple of times so that noise
    // doesn't fail an unrelated scenario; POST is never retried (it could create twice), and every retry
    // is logged so real flakiness stays visible.
    protected Task<IAPIResponse> GetAsync(string url, APIRequestContextOptions? options = null) =>
        SendWithRetryAsync("GET", url, () => Api.GetAsync(url, options));

    protected Task<IAPIResponse> PutAsync(string url, APIRequestContextOptions? options = null) =>
        SendWithRetryAsync("PUT", url, () => Api.PutAsync(url, options));

    protected Task<IAPIResponse> DeleteAsync(string url, APIRequestContextOptions? options = null) =>
        SendWithRetryAsync("DELETE", url, () => Api.DeleteAsync(url, options));

    private static async Task<IAPIResponse> SendWithRetryAsync(string method, string url, Func<Task<IAPIResponse>> send)
    {
        for (var attempt = 1; ; attempt++)
        {
            var response = await send();
            if (response.Status < 500 || attempt == MaxAttempts)
                return response;

            Console.Error.WriteLine($"[ApiClient] {method} {url} answered {response.Status}; retrying ({attempt}/{MaxAttempts - 1}).");
            await Task.Delay(TimeSpan.FromMilliseconds(300 * attempt));
        }
    }

    protected static string Url(string path, IReadOnlyDictionary<string, string?>? query = null)
    {
        var url = Settings.Current.ApiBaseUrl.TrimEnd('/') + path;
        if (query is null || query.Count == 0)
            return url;

        var pairs = query
            .Where(kv => kv.Value is not null)
            .Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value!)}");
        return url + "?" + string.Join("&", pairs);
    }

    protected static string Serialize<T>(T value) => JsonSerializer.Serialize(value, JsonDefaults.Options);

    /// <summary>Builds request headers for a JSON body, optionally bearing a Cognito access/id token.
    /// Several write endpoints (dishes/locations/tables) have no gateway auth at all, so the token is
    /// optional everywhere — passing one is harmless where it's ignored.</summary>
    protected static Dictionary<string, string> Headers(string? bearerToken = null)
    {
        var headers = new Dictionary<string, string> { ["Content-Type"] = "application/json" };
        if (!string.IsNullOrEmpty(bearerToken))
            headers["Authorization"] = $"Bearer {bearerToken}";
        return headers;
    }
}
