using Microsoft.Playwright;
using Restaurant_Aqa_Framework.Ui.Components;

namespace Restaurant_Aqa_Framework.Screenplay.Questions;

/// <summary>Who the storefront's header says is signed in (the label in the user menu), or null if nobody is.</summary>
public sealed class TheSignedInUser : IQuestion<string?>
{
    public async Task<string?> AnsweredByAsync(Actor actor)
    {
        var header = new HeaderComponent(actor.LookingAtThePage());
        await header.OpenUserMenuAsync();
        try
        {
            await header.SignedInUserLabel.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
            return (await header.SignedInUserLabel.TextContentAsync())?.Trim();
        }
        catch (TimeoutException)
        {
            return null;
        }
    }
}
