using Microsoft.Playwright;

namespace Restaurant_Aqa_Framework.Ui;

public sealed class LoginPage(IPage page) : BasePage(page)
{
    // The form's placeholder/ids have changed before, so locate by the visible label (email) and the
    // Angular form-control name (password — its label also wraps a "Toggle password visibility" button).
    public ILocator EmailInput => Page.GetByLabel("Email", new() { Exact = true });
    public ILocator PasswordInput => Page.Locator("input[formcontrolname='password']");
    public ILocator SubmitButton => Page.GetByRole(AriaRole.Button, new() { Name = "Sign In", Exact = true });
    public ILocator ErrorMessage => Page.Locator(".response-error-message");

    public Task NavigateAsync() => NavigateToAsync("/login");

    public async Task SignInAsync(string email, string password)
    {
        await EmailInput.FillAsync(email);
        await PasswordInput.FillAsync(password);
        await SubmitButton.ClickAsync();
    }
}
