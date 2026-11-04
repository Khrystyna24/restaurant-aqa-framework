using Microsoft.Playwright;
using Restaurant_Aqa_Framework.Configuration;

namespace Restaurant_Aqa_Framework.Screenplay.Tasks;

/// <summary>Opens the storefront's home page and remembers how the server answered.</summary>
public sealed class OpenTheHomePage : ITask
{
    public async Task PerformAsAsync(Actor actor)
    {
        var answer = await actor.LookingAtThePage().GotoAsync(Settings.Current.UiBaseUrl);
        if (answer is not null)
            actor.Remember(answer);
    }
}
