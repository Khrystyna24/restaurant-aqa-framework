namespace Restaurant_Aqa_Framework.Screenplay.Questions;

/// <summary>How the last request of the scenario turned out. Status and body are protocol detail: the step that
/// asks for them is the one place that turns them into a business statement ("the visitor is told …").</summary>
public sealed record Outcome(int Status, bool Succeeded, string ErrorMessage, string Body);

public sealed class TheOutcome : IQuestion<Outcome>
{
    public Task<Outcome> AnsweredByAsync(Actor actor)
    {
        var response = actor.TheLastResponse();
        return Task.FromResult(new Outcome(response.Status, response.Succeeded, response.ErrorMessage, response.Body));
    }
}
