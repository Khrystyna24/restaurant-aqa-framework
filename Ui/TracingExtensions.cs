using Microsoft.Playwright;

namespace Restaurant_Aqa_Framework.Ui;

public static class TracingExtensions
{
    /// <summary>
    /// Injects a message into the Playwright trace as an empty, immediately-closed group titled
    /// with <paramref name="message"/>, so custom markers show up in the trace viewer's timeline.
    /// </summary>
    public static async Task AddTraceMessage(this IBrowserContext browserContext, string message)
    {
        await browserContext.Tracing.GroupAsync(message);
        await browserContext.Tracing.GroupEndAsync();
    }

    public static Task TraceError(this IBrowserContext browserContext, string message) =>
        browserContext.AddTraceMessage($"❌ {message}");

    public static Task TraceSuccess(this IBrowserContext browserContext, string message) =>
        browserContext.AddTraceMessage($"✔ {message}");

    public static Task TraceInfo(this IBrowserContext browserContext, string message) =>
        browserContext.AddTraceMessage($"ℹ {message}");

    public static Task TraceWarning(this IBrowserContext browserContext, string message) =>
        browserContext.AddTraceMessage($"⚠ {message}");
}
