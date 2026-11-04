using Restaurant_Aqa_Framework.Ui;

namespace Restaurant_Aqa_Framework.Screenplay.Tasks;

public sealed class OpenTheSignUpPage : ITask
{
    public Task PerformAsAsync(Actor actor) => new RegisterPage(actor.LookingAtThePage()).NavigateAsync();
}
