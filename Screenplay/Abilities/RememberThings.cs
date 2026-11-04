namespace Restaurant_Aqa_Framework.Screenplay.Abilities;

/// <summary>
/// <see cref="IRemember"/> backed by Reqnroll's own <see cref="ScenarioContext"/> — through its typed
/// <c>Set</c>/<c>Get</c>/<c>TryGetValue</c> methods, never by indexing it like a dictionary. The context
/// belongs to exactly one scenario, so parallel scenarios cannot see each other's memories.
/// </summary>
public sealed class RememberThings(ScenarioContext scenario) : IRemember
{
    public void Remember<T>(T thing) where T : notnull
    {
        scenario.Set(thing);
        AddToHistory(thing);
    }

    public void Remember<T>(string name, T thing) where T : notnull
    {
        scenario.Set(thing, NamedKey<T>(name));
        Remember(thing);
    }

    public T Recall<T>() where T : notnull =>
        scenario.TryGetValue(out T? thing) ? thing! : throw Missing<T>();

    public T Recall<T>(string name) where T : notnull =>
        scenario.TryGetValue(NamedKey<T>(name), out T? thing) ? thing! : throw Missing<T>(name);

    public bool TryRecall<T>(out T thing) where T : notnull
    {
        var found = scenario.TryGetValue(out T? value);
        thing = value!;
        return found;
    }

    public IReadOnlyList<T> RecallAll<T>() where T : notnull =>
        scenario.TryGetValue(HistoryKey<T>(), out List<T>? all) ? all! : [];

    private void AddToHistory<T>(T thing) where T : notnull
    {
        if (!scenario.TryGetValue(HistoryKey<T>(), out List<T>? all))
        {
            all = [];
            scenario.Set(all, HistoryKey<T>());
        }

        all!.Add(thing);
    }

    private static string NamedKey<T>(string name) => $"{typeof(T).FullName}:{name}";

    private static string HistoryKey<T>() => $"all:{typeof(T).FullName}";

    private static InvalidOperationException Missing<T>(string? name = null) =>
        new($"This scenario has not set up any {typeof(T).Name}{(name is null ? "" : $" named '{name}'")} yet — " +
            "an earlier Given/When step has to create it before this step can use it.");
}
