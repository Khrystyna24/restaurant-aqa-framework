using Microsoft.Playwright;

namespace Restaurant_Aqa_Framework.Screenplay.Abilities;

/// <summary>Standalone API-only ability: a dedicated request context, no browser involved.</summary>
public sealed class CallTheApi(IAPIRequestContext api) : ICallTheApi
{
    public IAPIRequestContext Api { get; } = api;
}
