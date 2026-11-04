using Microsoft.Playwright;
using Restaurant_Aqa_Framework.Support;

namespace Restaurant_Aqa_Framework.Ui;

public sealed class RegisterPage(IPage page) : BasePage(page)
{
    public ILocator FirstNameInput => Page.Locator("#firstName");
    public ILocator LastNameInput => Page.Locator("#lastName");
    public ILocator EmailInput => Page.Locator("#email");
    /// <summary>Tag-qualified: `app-password-with-rules` passes `placeholder` as a static host
    /// attribute (not just to its inner `&lt;input&gt;`), so an untagged `GetByPlaceholder` match
    /// is ambiguous between the component host and the real input.</summary>
    public ILocator PasswordInput => Page.Locator("input[placeholder='Enter your Password']");
    public ILocator ConfirmPasswordInput => Page.GetByPlaceholder("Confirm your Password");
    public ILocator SubmitButton => Page.GetByRole(AriaRole.Button, new() { Name = "Create an Account" });
    /// <summary>Every per-field validation message currently shown under the form.</summary>
    public ILocator FieldErrors => Page.Locator(".error-message");

    /// <summary>A per-field validation message under the form, e.g. "Email is required.".</summary>
    public ILocator FieldError(string text) => Page.Locator(".error-message").Filter(new() { HasText = text });

    public ILocator ErrorMessage => Page.Locator(".response-error-message");

    public Task NavigateAsync() => NavigateToAsync("/signup");

    public ILocator Input(InputField field) => field switch
    {
        InputField.FirstName => FirstNameInput,
        InputField.LastName => LastNameInput,
        InputField.EmailAddress => EmailInput,
        InputField.Password => PasswordInput,
        InputField.PasswordConfirmation => ConfirmPasswordInput,
        _ => throw new ArgumentOutOfRangeException(nameof(field), field, null),
    };

    public Task FillAsync(InputField field, string value) => Input(field).FillAsync(value);

    public async Task FillAllAsync(string firstName, string lastName, string email, string password, string confirmPassword)
    {
        await FillAsync(InputField.FirstName, firstName);
        await FillAsync(InputField.LastName, lastName);
        await FillAsync(InputField.EmailAddress, email);
        await FillAsync(InputField.Password, password);
        await FillAsync(InputField.PasswordConfirmation, confirmPassword);
    }

    public async Task RegisterAsync(string firstName, string lastName, string email, string password)
    {
        await FillAllAsync(firstName, lastName, email, password, password);
        await SubmitButton.ClickAsync();
    }
}
