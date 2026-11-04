using Restaurant_Aqa_Framework.Api.Dtos.Auth;

namespace Restaurant_Aqa_Framework.Screenplay.Questions;

/// <summary>The id token the last sign-in handed out, or null if that sign-in did not succeed.</summary>
public sealed class TheIdToken : IQuestion<string?>
{
    public Task<string?> AnsweredByAsync(Actor actor)
    {
        var response = actor.TheLastResponse();
        return Task.FromResult(response.Succeeded ? response.Json<SignInResponse>().IdToken : null);
    }
}
