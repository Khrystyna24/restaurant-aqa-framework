namespace Restaurant_Aqa_Framework.Screenplay;

/// <summary>Something an actor finds out without changing anything.</summary>
public interface IQuestion<TAnswer>
{
    Task<TAnswer> AnsweredByAsync(Actor actor);
}
