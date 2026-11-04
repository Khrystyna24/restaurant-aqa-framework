using Microsoft.Playwright;

namespace Restaurant_Aqa_Framework.Screenplay.Abilities;

/// <summary>Ability to drive the browser for a `@ui` scenario.</summary>
public interface IBrowseTheWeb
{
    IBrowserContext Context { get; }
    IPage Page { get; }
}
