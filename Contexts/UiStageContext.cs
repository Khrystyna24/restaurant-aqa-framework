using Microsoft.Playwright;
using Restaurant_Aqa_Framework.Screenplay;

namespace Restaurant_Aqa_Framework.Contexts;

/// <summary>
/// `@ui` stage: a dedicated `IBrowserContext` + `IPage`. The actor can both
/// browse the web and call the API (via `BrowseTheWeb`, which implements both
/// abilities), so a mixed UI+API scenario only ever needs the `@ui` tag.
/// </summary>
public sealed class UiStageContext(IBrowserContext browserContext, Actor actor) : IStageContext
{
    public IBrowserContext BrowserContext { get; } = browserContext;
    public Actor Actor { get; } = actor;

    public async ValueTask DisposeAsync() => await BrowserContext.CloseAsync();
}
