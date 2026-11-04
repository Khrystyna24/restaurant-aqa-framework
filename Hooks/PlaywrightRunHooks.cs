using Microsoft.Playwright;
using Restaurant_Aqa_Framework.Configuration;
using Restaurant_Aqa_Framework.Core;

namespace Restaurant_Aqa_Framework.Hooks;

/// <summary>
/// One `IPlaywright` + one `IBrowser` for the entire test run. Reqnroll runs
/// `[BeforeTestRun]`/`[AfterTestRun]` exactly once, blocking all parallel
/// workers until done, so this is safe under TUnit's default parallelism —
/// scenario-level isolation comes from a fresh `IBrowserContext` per `@ui`
/// scenario (see <see cref="ScenarioStageHooks"/>), not from the browser itself.
/// </summary>
[Binding]
public static class PlaywrightRunHooks
{
    public static IPlaywright Playwright { get; private set; } = null!;
    public static IBrowser Browser { get; private set; } = null!;

    [BeforeTestRun]
    public static async Task StartBrowser()
{
    var settings = Settings.Current;
    ArtifactPaths.ClearTraces();

    var channel = Environment.GetEnvironmentVariable("CHANNEL");
    var useChannel = settings.Browser == BrowserKind.Chromium
                     && !string.IsNullOrWhiteSpace(channel);

    if (!useChannel)
        await PlaywrightInstaller.EnsureInstalledAsync(settings.Browser.ToString().ToLowerInvariant());

    Playwright = await Microsoft.Playwright.Playwright.CreateAsync();

    Browser = settings.Browser switch
    {
        BrowserKind.Firefox => await Playwright.Firefox.LaunchAsync(new BrowserTypeLaunchOptions { Headless = settings.Headless }),
        BrowserKind.Webkit => await Playwright.Webkit.LaunchAsync(new BrowserTypeLaunchOptions { Headless = settings.Headless }),
        _ => await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = settings.Headless,
            Channel = useChannel ? channel : null
        }),
    };
}

    [AfterTestRun]
    public static async Task StopBrowser()
    {
        if (Browser is not null)
            await Browser.CloseAsync();
        Playwright?.Dispose();
    }
}
