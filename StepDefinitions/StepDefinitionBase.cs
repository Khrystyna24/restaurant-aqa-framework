using Restaurant_Aqa_Framework.Screenplay;

namespace Restaurant_Aqa_Framework.StepDefinitions;

/// <summary>
/// Common ground for step-definition classes. A step has exactly one collaborator, the scenario's actor: it
/// asks the actor to attempt a <see cref="ITask"/> (<c>Actor.AttemptsToAsync</c>) or to answer an
/// <see cref="IQuestion{TAnswer}"/> (<c>Actor.AsksForAsync</c>) and asserts on the answer. Tasks and questions are
/// the only code that touches API clients and page objects. A step class holds no state of its own: what an
/// earlier step created is in the actor's memory, read here with <see cref="Recall{T}"/>.
/// </summary>
public abstract class StepDefinitionBase(Actor actor)
{
    protected Actor Actor { get; } = actor;

    /// <summary>The current <typeparamref name="T"/> from the actor's memory, or the one remembered under <paramref name="name"/>.</summary>
    protected T Recall<T>(string? name = null) where T : notnull => Actor.Recall<T>(name);
}
