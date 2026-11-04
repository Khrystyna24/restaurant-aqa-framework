using Restaurant_Aqa_Framework.Ui;

namespace Restaurant_Aqa_Framework.Screenplay.Tasks;

/// <summary>Signs in through the storefront's login form and waits until it has moved on from the login page.</summary>
public sealed class SignInThroughTheLoginForm(string email, string password, bool openTheLoginPage = true) : ITask
{
    public async Task PerformAsAsync(Actor actor)
    {
        var page = actor.LookingAtThePage();
        var loginPage = new LoginPage(page);

        if (openTheLoginPage)
            await loginPage.NavigateAsync();

        await loginPage.SignInAsync(email, password);
        await page.WaitForURLAsync(url => !url.Contains("/login", StringComparison.Ordinal));
    }
}
