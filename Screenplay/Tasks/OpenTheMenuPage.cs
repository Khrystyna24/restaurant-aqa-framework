using Restaurant_Aqa_Framework.Ui;

namespace Restaurant_Aqa_Framework.Screenplay.Tasks;

public sealed class OpenTheMenuPage : ITask
{
    public Task PerformAsAsync(Actor actor) => new MenuPage(actor.LookingAtThePage()).NavigateAsync();
}
