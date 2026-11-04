using Microsoft.Playwright;
using Restaurant_Aqa_Framework.Contexts;
using Restaurant_Aqa_Framework.Screenplay.Abilities;

namespace Restaurant_Aqa_Framework.Screenplay;

public static class ActorExtensions
{
    // ---- memory ----

    public static void Remember<T>(this Actor actor, T thing) where T : notnull =>
        actor.AbilityTo<IRemember>().Remember(thing);

    public static void Remember<T>(this Actor actor, string name, T thing) where T : notnull =>
        actor.AbilityTo<IRemember>().Remember(name, thing);

    public static T Recall<T>(this Actor actor) where T : notnull => actor.AbilityTo<IRemember>().Recall<T>();

    /// <summary>The current <typeparamref name="T"/>, or the one remembered under <paramref name="name"/> when a name is given.</summary>
    public static T Recall<T>(this Actor actor, string? name) where T : notnull =>
        name is null ? actor.AbilityTo<IRemember>().Recall<T>() : actor.AbilityTo<IRemember>().Recall<T>(name);

    public static bool TryRecall<T>(this Actor actor, out T thing) where T : notnull =>
        actor.AbilityTo<IRemember>().TryRecall(out thing);

    public static IReadOnlyList<T> RecallAll<T>(this Actor actor) where T : notnull =>
        actor.AbilityTo<IRemember>().RecallAll<T>();

    // ---- what the actor works with ----

    /// <summary>The request context the actor calls the API with (for tasks and questions, never for step definitions).</summary>
    public static IAPIRequestContext UsingTheApi(this Actor actor) => actor.AbilityTo<ICallTheApi>().Api;

    /// <summary>The page the actor is looking at (only a <c>@ui</c> actor has one).</summary>
    public static IPage LookingAtThePage(this Actor actor) => actor.AbilityTo<IBrowseTheWeb>().Page;

    /// <summary>The scenario's last API response; tasks leave their outcome here and questions read it.</summary>
    public static ApiResponseContext TheLastResponse(this Actor actor) => actor.Recall<ApiResponseContext>();

    // ---- doing and asking ----

    public static async Task AttemptsToAsync(this Actor actor, params ITask[] tasks)
    {
        foreach (var task in tasks)
            await task.PerformAsAsync(actor);
    }

    public static Task<TAnswer> AsksForAsync<TAnswer>(this Actor actor, IQuestion<TAnswer> question) =>
        question.AnsweredByAsync(actor);
}
