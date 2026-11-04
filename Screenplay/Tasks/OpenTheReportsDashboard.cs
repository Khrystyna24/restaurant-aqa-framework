using Restaurant_Aqa_Framework.Ui;

namespace Restaurant_Aqa_Framework.Screenplay.Tasks;

public sealed class OpenTheReportsDashboard : ITask
{
    public Task PerformAsAsync(Actor actor) => new ReportsPage(actor.LookingAtThePage()).NavigateAsync();
}
