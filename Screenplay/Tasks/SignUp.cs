using Restaurant_Aqa_Framework.Api.Dtos.Auth;
using Restaurant_Aqa_Framework.Ui;

namespace Restaurant_Aqa_Framework.Screenplay.Tasks;

/// <summary>Signs a visitor up through the storefront's form and waits until the storefront sends them to sign in
/// (it never signs a new customer in by itself). Remembers who the visitor is.</summary>
public sealed class SignUp(RegisterRequest visitor) : ITask
{
    public async Task PerformAsAsync(Actor actor)
    {
        var page = actor.LookingAtThePage();
        actor.Remember(visitor);

        await new RegisterPage(page).RegisterAsync(visitor.FirstName, visitor.LastName, visitor.Email, visitor.Password);
        await page.WaitForURLAsync("**/login");
    }
}
