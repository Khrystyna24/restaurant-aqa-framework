using Microsoft.Playwright;
using Restaurant_Aqa_Framework.Ui;

namespace Restaurant_Aqa_Framework.Screenplay.Questions;

/// <summary>Whether the storefront's menu page shows a dish of this name (waiting for the page to render it).</summary>
public sealed class TheDishOnTheMenu(string dishName) : IQuestion<bool>
{
    public async Task<bool> AnsweredByAsync(Actor actor)
    {
        try
        {
            await new MenuPage(actor.LookingAtThePage()).Dish(dishName).Name
                .WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
            return true;
        }
        catch (TimeoutException)
        {
            return false;
        }
    }
}
