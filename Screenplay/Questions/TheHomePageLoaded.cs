using Microsoft.Playwright;

namespace Restaurant_Aqa_Framework.Screenplay.Questions;

/// <summary>Whether the server answered the home page request successfully.</summary>
public sealed class TheHomePageLoaded : IQuestion<bool>
{
    public Task<bool> AnsweredByAsync(Actor actor) =>
        Task.FromResult(actor.TryRecall<IResponse>(out var answer) && answer.Ok);
}
