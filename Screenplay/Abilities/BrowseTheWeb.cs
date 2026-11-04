using Microsoft.Playwright;

namespace Restaurant_Aqa_Framework.Screenplay.Abilities;

/// <summary>
/// `@ui` scenario ability: browses the web, and can also call the API through
/// the same browser context — so API calls made this way share cookies/session
/// with whatever the page has done (e.g. a JWT written to localStorage by the
/// UI's own login flow is irrelevant here, but shared cookies are not).
/// </summary>
public sealed class BrowseTheWeb(IBrowserContext context, IPage page) : IBrowseTheWeb, ICallTheApi
{
    public IBrowserContext Context { get; } = context;
    public IPage Page { get; } = page;
    public IAPIRequestContext Api => Context.APIRequest;
}
