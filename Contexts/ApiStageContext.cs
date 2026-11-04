using Microsoft.Playwright;
using Restaurant_Aqa_Framework.Screenplay;

namespace Restaurant_Aqa_Framework.Contexts;

/// <summary>`@api` stage: a standalone `IAPIRequestContext`. No browser is ever launched for this stage.</summary>
public sealed class ApiStageContext(IAPIRequestContext apiRequestContext, Actor actor) : IStageContext
{
    public IAPIRequestContext ApiRequestContext { get; } = apiRequestContext;
    public Actor Actor { get; } = actor;

    public async ValueTask DisposeAsync() => await ApiRequestContext.DisposeAsync();
}
