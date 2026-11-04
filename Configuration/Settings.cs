using dotenv.net.Utilities;

namespace Restaurant_Aqa_Framework.Configuration;

/// <summary>Which browser engine Playwright launches for `@ui` scenarios.</summary>
public enum BrowserKind
{
    Chromium,
    Firefox,
    Webkit,
}

/// <summary>
/// Controls when a Playwright trace/screenshot is attached to the test report.
/// `OnFailureOnly` is the default so passing runs stay lightweight; a scenario
/// tagged `@trace-always`, or this set to `Always`, keeps the artifact regardless
/// of outcome.
/// </summary>
public enum TraceMode
{
    OnFailureOnly,
    Always,
    Never,
}

/// <summary>Pass/fail budgets (PERF_* in `.env`) for page-load and API-response performance checks.</summary>
public sealed record PerformanceThresholds(int PageLoadMs, int ApiResponseMs);

/// <summary>
/// Strongly-typed, validated view over configuration loaded from `.env` / real
/// environment variables. Required values fail fast with an actionable message;
/// everything else has a sensible default. Access via <see cref="Current"/>.
/// </summary>
public sealed record Settings(
    string UiBaseUrl,
    string ApiBaseUrl,
    BrowserKind Browser,
    bool Headless,
    int DefaultTimeoutMs,
    int NavigationTimeoutMs,
    TraceMode TraceMode,
    PerformanceThresholds Performance,
    string? WaiterUsername,
    string? WaiterPassword,
    string? AdminUsername,
    string? AdminPassword,
    string? TestTableId,
    string? TestTableWaiterUsername,
    string? TestTableWaiterPassword)
{
    private static readonly Lazy<Settings> Lazy = new(Load);

    public static Settings Current => Lazy.Value;

    /// <summary>The standing Waiter account, or a clear failure when it isn't configured
    /// (a Waiter cannot be self-provisioned — `POST /register` always yields a Customer).</summary>
    public (string Username, string Password) RequireWaiter() =>
        WaiterUsername is { Length: > 0 } user && WaiterPassword is { Length: > 0 } password
            ? (user, password)
            : throw new InvalidOperationException(
                "WAITER_USERNAME/WAITER_PASSWORD are not set. Set them in '.env' or as environment variables. See '.env.example'.");

    private static Settings Load()
    {
        EnvConfig.EnsureLoaded();

        return new Settings(
            UiBaseUrl: RequireString("UI_URL"),
            ApiBaseUrl: RequireString("API_URL"),
            Browser: ParseEnum("BROWSER", BrowserKind.Chromium),
            Headless: ReadBool("HEADLESS", true),
            DefaultTimeoutMs: ReadInt("DEFAULT_TIMEOUT_MS", 15_000),
            NavigationTimeoutMs: ReadInt("NAVIGATION_TIMEOUT_MS", 30_000),
            TraceMode: ParseEnum("TRACE_MODE", TraceMode.OnFailureOnly),
            Performance: new PerformanceThresholds(
                PageLoadMs: ReadInt("PERF_PAGE_LOAD_MS", 3_000),
                ApiResponseMs: ReadInt("PERF_API_RESPONSE_MS", 1_000)),
            WaiterUsername: ReadOptionalString("WAITER_USERNAME"),
            WaiterPassword: ReadOptionalString("WAITER_PASSWORD"),
            AdminUsername: ReadOptionalString("ADMIN_USERNAME"),
            AdminPassword: ReadOptionalString("ADMIN_PASSWORD"),
            TestTableId: ReadOptionalString("TESTTABLE_ID"),
            TestTableWaiterUsername: ReadOptionalString("TESTTABLE_WAITER_USERNAME"),
            TestTableWaiterPassword: ReadOptionalString("TESTTABLE_WAITER_PASSWORD"));
    }

    private static string RequireString(string key)
    {
        if (EnvReader.TryGetStringValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
            return value;

        throw new InvalidOperationException(
            $"Missing required configuration '{key}'. Set it in '.env' or as an environment variable. " +
            "See '.env.example' for the full list of supported settings.");
    }

    private static string? ReadOptionalString(string key)
        => EnvReader.TryGetStringValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value : null;

    private static bool ReadBool(string key, bool fallback)
        => EnvReader.TryGetBooleanValue(key, out var value) ? value : fallback;

    private static int ReadInt(string key, int fallback)
        => EnvReader.TryGetIntValue(key, out var value) ? value : fallback;

    private static TEnum ParseEnum<TEnum>(string key, TEnum fallback) where TEnum : struct, Enum
    {
        if (!EnvReader.TryGetStringValue(key, out var raw) || string.IsNullOrWhiteSpace(raw))
            return fallback;

        if (Enum.TryParse<TEnum>(raw, ignoreCase: true, out var parsed))
            return parsed;

        var allowed = string.Join(", ", Enum.GetNames<TEnum>());
        throw new InvalidOperationException(
            $"Invalid value '{raw}' for configuration '{key}'. Allowed values: {allowed}.");
    }
}
