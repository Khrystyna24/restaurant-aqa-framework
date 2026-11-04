using Restaurant_Aqa_Framework.Configuration;
using Restaurant_Aqa_Framework.Screenplay;
using Restaurant_Aqa_Framework.Screenplay.Questions;
using Restaurant_Aqa_Framework.Screenplay.Tasks;
using Restaurant_Aqa_Framework.Support;

namespace Restaurant_Aqa_Framework.StepDefinitions;

[Binding]
public sealed class AuthStepDefinitions(Actor actor) : StepDefinitionBase(actor)
{
    private const string UnknownVisitorEmail = "no.such.user@example.com";
    private const string ArbitraryValidEmail = "someone@example.com";
    private const string ArbitraryValidPassword = "Secret123!";
    private const string WrongPassword = "Wr0ng-Passw0rd!";

    [Given("the restaurant has a waiter account")]
    public static void GivenRestaurantHasAWaiterAccount() => Settings.Current.RequireWaiter();

    // The same sentence means "use the sign-in service" on an @api stage and "use the login form" on a
    // @ui stage — scoped bindings keep the feature file free of that distinction.
    [When("the waiter signs in with the correct password")]
    [Scope(Tag = "api")]
    public Task WhenWaiterSignsInThroughTheService()
    {
        var (username, password) = Settings.Current.RequireWaiter();
        return Actor.AttemptsToAsync(new SignInThroughTheService(username, password));
    }

    [When("the waiter signs in with the correct password")]
    [Scope(Tag = "ui")]
    public Task WhenWaiterSignsInThroughTheLoginForm()
    {
        var (username, password) = Settings.Current.RequireWaiter();
        return Actor.AttemptsToAsync(new SignInThroughTheLoginForm(username, password));
    }

    [When("the waiter signs in with a wrong password")]
    public Task WhenWaiterSignsInWithWrongPassword()
    {
        var (username, _) = Settings.Current.RequireWaiter();
        return Actor.AttemptsToAsync(new SignInThroughTheService(username, WrongPassword));
    }

    [When("someone who never registered tries to sign in")]
    public Task WhenUnknownVisitorTriesToSignIn() =>
        Actor.AttemptsToAsync(new SignInThroughTheService(UnknownVisitorEmail, WrongPassword));

    /// <summary>Everything except the named field is valid, so the field under test is the only possible cause.</summary>
    [When("someone tries to sign in with an invalid {field} \\({problem}\\)")]
    public Task WhenSomeoneTriesToSignInWithInvalidField(InputField field, InputProblem problem)
    {
        var bad = InvalidInputs.Value(field, problem);
        return field switch
        {
            InputField.EmailAddress => Actor.AttemptsToAsync(new SignInThroughTheService(bad, ArbitraryValidPassword)),
            InputField.Password => Actor.AttemptsToAsync(new SignInThroughTheService(ArbitraryValidEmail, bad)),
            _ => throw new NotSupportedException($"The sign-in form has no '{field}'."),
        };
    }

    // ---- admin: no admin account is provided to this environment yet, so these scenarios show up as skipped ----

    [Given("the restaurant has an admin account")]
    public static void GivenRestaurantHasAnAdminAccount()
    {
        var settings = Settings.Current;
        if (string.IsNullOrEmpty(settings.AdminUsername) || string.IsNullOrEmpty(settings.AdminPassword))
            Skip.Test("No admin account is configured (ADMIN_USERNAME / ADMIN_PASSWORD).");
    }

    [When("the admin signs in")]
    public Task WhenAdminSignsIn() =>
        Actor.AttemptsToAsync(new SignInThroughTheLoginForm(Settings.Current.AdminUsername!, Settings.Current.AdminPassword!));

    [When("the admin opens the reports dashboard")]
    [When("a guest opens the reports dashboard")]
    public Task WhenSomeoneOpensTheReportsDashboard() => Actor.AttemptsToAsync(new OpenTheReportsDashboard());

    [Then("the reports dashboard is shown")]
    public async Task ThenReportsDashboardIsShown() =>
        await Assert.That(await Actor.AsksForAsync(new TheReportsDashboardIsShown(patiently: true))).IsTrue();

    [Then("the reports dashboard is not shown")]
    public async Task ThenReportsDashboardIsNotShown() =>
        await Assert.That(await Actor.AsksForAsync(new TheReportsDashboardIsShown(patiently: false))).IsFalse();

    [Then("the waiter is signed in")]
    [Scope(Tag = "api")]
    public async Task ThenWaiterIsSignedInThroughTheService()
    {
        var outcome = await Actor.AsksForAsync(new TheOutcome());

        await Assert.That(outcome.Succeeded).IsTrue().Because($"response body was: {outcome.Body}");
        await Assert.That(await Actor.AsksForAsync(new TheIdToken())).IsNotNullOrEmpty();
    }

    // A waiter lands on the waiter dashboard, whose greeting names the role ("Hello, <name> (Waiter)") —
    // unlike a customer's header it has no user dropdown to open, so the greeting is the sign-in proof.
    [Then("the waiter is signed in")]
    [Scope(Tag = "ui")]
    public async Task ThenWaiterIsSignedInOnTheStorefront() =>
        await Assert.That(await Actor.AsksForAsync(new TheGreeting())).Contains("(Waiter)");
}
