using Microsoft.Playwright;

namespace Restaurant_Aqa_Framework.Ui;

/// <summary>Base for flat page objects — exposes the live <see cref="IPage"/> shared with whatever
/// `IBrowseTheWeb` ability constructed it, plus a helper to navigate to an absolute (site-rooted)
/// path. Relies on the browser context's own <c>BaseURL</c> (set in `ScenarioStageHooks` from
/// `Settings.UiBaseUrl`, which may itself include a path like `/main`) and standard URL resolution
/// rules — a leading `/` replaces the base's path entirely rather than appending to it, so this
/// stays correct even though `UiBaseUrl` isn't a bare origin. Named <see cref="NavigateToAsync"/>
/// rather than <c>NavigateAsync</c> so derived pages are free to expose their own, more specific
/// <c>NavigateAsync()</c> overloads without colliding with this one.</summary>
public abstract class BasePage(IPage page)
{
    protected IPage Page { get; } = page;

    protected Task<IResponse?> NavigateToAsync(string path) => Page.GotoAsync(path);
}
