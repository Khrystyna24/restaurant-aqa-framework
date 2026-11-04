namespace Restaurant_Aqa_Framework.Screenplay;

/// <summary>
/// A Screenplay-pattern actor: a named entity that performs Tasks using whatever
/// Abilities it was given. Abilities are resolved by interface type, so a step
/// definition asking for <c>actor.AbilityTo&lt;ICallTheApi&gt;()</c> doesn't care
/// whether that ability came from a `@ui` or `@api` scenario stage.
/// </summary>
public sealed class Actor
{
    private readonly Dictionary<Type, object> _abilities = new();

    private Actor(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public static Actor Named(string name) => new(name);

    /// <summary>
    /// Always pass the ability interface explicitly, e.g. <c>WhoCan&lt;ICallTheApi&gt;(impl)</c>.
    /// Without it, TAbility infers as the concrete implementation type, which <see cref="AbilityTo{TAbility}"/>
    /// (looked up by interface) will never find.
    /// </summary>
    public Actor WhoCan<TAbility>(TAbility ability) where TAbility : notnull
    {
        _abilities[typeof(TAbility)] = ability;
        return this;
    }

    public TAbility AbilityTo<TAbility>() where TAbility : notnull
    {
        if (_abilities.TryGetValue(typeof(TAbility), out var ability))
            return (TAbility)ability;

        throw new InvalidOperationException(
            $"{Name} has no ability '{typeof(TAbility).Name}'. " +
            "Check the scenario's @ui/@api tag — each stage only grants the abilities it supports.");
    }

    public bool Can<TAbility>() where TAbility : notnull => _abilities.ContainsKey(typeof(TAbility));
}
