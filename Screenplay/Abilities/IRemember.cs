namespace Restaurant_Aqa_Framework.Screenplay.Abilities;

/// <summary>
/// The actor's memory for the length of one scenario: the locations it set up, the dish it added, the waiter
/// session it opened. It is how steps in different step-definition classes share what earlier steps created.
///
/// Anything can be remembered several times. The most recent one is "the current" <typeparamref name="T"/>
/// (what an unnamed <see cref="Recall{T}()"/> returns); give a thing a name to fetch it later by that name, and
/// <see cref="RecallAll{T}"/> returns every one remembered so far, oldest first.
/// </summary>
public interface IRemember
{
    /// <summary>Remembers a <typeparamref name="T"/> and makes it the current one.</summary>
    void Remember<T>(T thing) where T : notnull;

    /// <summary>Remembers a <typeparamref name="T"/> under a name (and makes it the current one).</summary>
    void Remember<T>(string name, T thing) where T : notnull;

    /// <summary>The current <typeparamref name="T"/>; fails with a message naming what is missing.</summary>
    T Recall<T>() where T : notnull;

    /// <summary>The <typeparamref name="T"/> remembered under <paramref name="name"/>.</summary>
    T Recall<T>(string name) where T : notnull;

    bool TryRecall<T>(out T thing) where T : notnull;

    /// <summary>Every <typeparamref name="T"/> remembered in this scenario, oldest first.</summary>
    IReadOnlyList<T> RecallAll<T>() where T : notnull;
}
