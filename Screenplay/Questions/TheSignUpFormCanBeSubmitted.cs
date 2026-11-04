using Restaurant_Aqa_Framework.Ui;

namespace Restaurant_Aqa_Framework.Screenplay.Questions;

/// <summary>Whether the sign-up form currently lets the visitor submit it. The form re-validates as the visitor types and
/// the submit button follows a moment later, so the answer is the button's state once it has stopped changing —
/// not whatever it happened to show the instant after the last keystroke.</summary>
public sealed class TheSignUpFormCanBeSubmitted : IQuestion<bool>
{
    private static readonly TimeSpan Poll = TimeSpan.FromMilliseconds(100);
    private const int StableSamples = 4;
    private static readonly TimeSpan GiveUpAfter = TimeSpan.FromSeconds(5);

    public async Task<bool> AnsweredByAsync(Actor actor)
    {
        var submit = new RegisterPage(actor.LookingAtThePage()).SubmitButton;
        var started = DateTime.UtcNow;
        var current = await submit.IsEnabledAsync();
        var sameSamples = 1;

        while (sameSamples < StableSamples && DateTime.UtcNow - started < GiveUpAfter)
        {
            await Task.Delay(Poll);
            var next = await submit.IsEnabledAsync();
            sameSamples = next == current ? sameSamples + 1 : 1;
            current = next;
        }

        return current;
    }
}
