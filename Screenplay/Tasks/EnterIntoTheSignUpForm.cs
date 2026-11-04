using Restaurant_Aqa_Framework.Support;
using Restaurant_Aqa_Framework.Ui;

namespace Restaurant_Aqa_Framework.Screenplay.Tasks;

/// <summary>Replaces what is in one field of the sign-up form.</summary>
public sealed class EnterIntoTheSignUpForm(InputField field, string value) : ITask
{
    public Task PerformAsAsync(Actor actor) => new RegisterPage(actor.LookingAtThePage()).FillAsync(field, value);
}
