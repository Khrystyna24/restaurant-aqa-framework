namespace Restaurant_Aqa_Framework.Screenplay;

/// <summary>The standing waiter's id token, remembered once per scenario for calls that need to be authorised
/// (removing what the scenario created).</summary>
public sealed record WaiterSession(string IdToken);
