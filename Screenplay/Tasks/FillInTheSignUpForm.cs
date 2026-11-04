using Restaurant_Aqa_Framework.Api.Dtos.Auth;
using Restaurant_Aqa_Framework.Ui;

namespace Restaurant_Aqa_Framework.Screenplay.Tasks;

/// <summary>Fills in every field of the sign-up form without submitting it, and remembers who the visitor is.</summary>
public sealed class FillInTheSignUpForm(RegisterRequest visitor) : ITask
{
    public Task PerformAsAsync(Actor actor)
    {
        actor.Remember(visitor);
        return new RegisterPage(actor.LookingAtThePage())
            .FillAllAsync(visitor.FirstName, visitor.LastName, visitor.Email, visitor.Password, visitor.ConfirmPassword);
    }
}
