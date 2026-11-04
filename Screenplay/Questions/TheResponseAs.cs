namespace Restaurant_Aqa_Framework.Screenplay.Questions;

/// <summary>What the last successful request of the scenario returned, read as a <typeparamref name="T"/>. Fails with the
/// status and body if the request did not succeed.</summary>
public sealed class TheResponseAs<T> : IQuestion<T>
{
    public Task<T> AnsweredByAsync(Actor actor) => Task.FromResult(actor.TheLastResponse().Json<T>());
}
