using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Restaurant_Aqa_Framework.Screenplay.Questions;

/// <summary>The dashboard greeting a signed-in staff member sees ("Hello, &lt;name&gt; (Waiter)"), or null if the page shows
/// none. Staff land on a dashboard whose greeting names their role, unlike a customer's header.</summary>
public sealed class TheGreeting : IQuestion<string?>
{
    public async Task<string?> AnsweredByAsync(Actor actor)
    {
        var greeting = actor.LookingAtThePage().GetByRole(AriaRole.Heading, new() { NameRegex = new Regex("^Hello,") });
        try
        {
            await greeting.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
            return (await greeting.TextContentAsync())?.Trim();
        }
        catch (TimeoutException)
        {
            return null;
        }
    }
}
