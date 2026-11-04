using Microsoft.Playwright;
using Restaurant_Aqa_Framework.Ui;

namespace Restaurant_Aqa_Framework.Screenplay.Questions;

/// <summary>The explanations the sign-up form currently shows under its fields (empty if it shows none).</summary>
public sealed class TheSignUpFormMessages : IQuestion<IReadOnlyList<string>>
{
    public async Task<IReadOnlyList<string>> AnsweredByAsync(Actor actor)
    {
        var messages = new RegisterPage(actor.LookingAtThePage()).FieldErrors;
        try
        {
            await messages.First.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
        }
        catch (TimeoutException)
        {
            return [];
        }

        return (await messages.AllTextContentsAsync()).Select(text => text.Trim()).ToList();
    }
}
