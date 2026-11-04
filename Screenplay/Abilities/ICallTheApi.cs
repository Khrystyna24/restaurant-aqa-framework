using Microsoft.Playwright;

namespace Restaurant_Aqa_Framework.Screenplay.Abilities;

/// <summary>
/// Ability to call the backend API. `Api` always needs a fully-qualified URL
/// (built from `Settings.Current.ApiBaseUrl`) — the request context backing
/// this ability may be a `@ui` scenario's shared browser context (whose
/// base URL is the frontend, for page navigation) or a standalone `@api`
/// context, and only the latter has the backend as its base URL.
/// </summary>
public interface ICallTheApi
{
    IAPIRequestContext Api { get; }
}
