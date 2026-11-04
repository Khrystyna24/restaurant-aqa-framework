using System.Text.Json;

namespace Restaurant_Aqa_Framework.Screenplay.Questions;

/// <summary>The last successful response as raw JSON, for comparing named facts without a dedicated type.</summary>
public sealed class TheResponseJson : IQuestion<JsonElement>
{
    public Task<JsonElement> AnsweredByAsync(Actor actor) => Task.FromResult(actor.TheLastResponse().JsonElement());
}
