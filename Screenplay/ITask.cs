namespace Restaurant_Aqa_Framework.Screenplay;

/// <summary>Something an actor does that changes the world — set up a location, add a dish. A task uses only
/// the actor's abilities and memory, so any step definition in any class can perform it.</summary>
public interface ITask
{
    Task PerformAsAsync(Actor actor);
}
