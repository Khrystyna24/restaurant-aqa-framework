using Restaurant_Aqa_Framework.Api;

namespace Restaurant_Aqa_Framework.Screenplay.Questions;

/// <summary>The waiter session of this scenario — signs in the first time it is asked, then remembers it.</summary>
public sealed class TheWaiterSession : IQuestion<WaiterSession>
{
    public async Task<WaiterSession> AnsweredByAsync(Actor actor)
    {
        if (actor.TryRecall<WaiterSession>(out var session))
            return session;

        session = new WaiterSession(await new AuthApi(actor.UsingTheApi()).SignInAsWaiterAsync());
        actor.Remember(session);
        return session;
    }
}
