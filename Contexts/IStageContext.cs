using Restaurant_Aqa_Framework.Screenplay;

namespace Restaurant_Aqa_Framework.Contexts;

/// <summary>
/// What a scenario stage (`@ui` or `@api`) owns for the lifetime of one scenario:
/// the actor playing it, and whatever Playwright resources back its abilities,
/// torn down together when the scenario ends.
/// </summary>
public interface IStageContext : IAsyncDisposable
{
    Actor Actor { get; }
}
