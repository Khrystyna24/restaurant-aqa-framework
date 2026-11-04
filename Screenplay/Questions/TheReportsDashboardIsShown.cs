using Microsoft.Playwright;
using Restaurant_Aqa_Framework.Ui;

namespace Restaurant_Aqa_Framework.Screenplay.Questions;

/// <summary>Whether the reports dashboard is on screen. When the answer is expected to be yes, ask <c>patiently</c> — the
/// page needs a moment to render. When it is expected to be no, ask once the page has finished loading instead of
/// waiting out a timeout.</summary>
public sealed class TheReportsDashboardIsShown(bool patiently) : IQuestion<bool>
{
    public async Task<bool> AnsweredByAsync(Actor actor)
    {
        var page = actor.LookingAtThePage();
        var dashboard = new ReportsPage(page).Dashboard;

        if (!patiently)
        {
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            return await dashboard.CountAsync() > 0;
        }

        try
        {
            await dashboard.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
            return true;
        }
        catch (TimeoutException)
        {
            return false;
        }
    }
}
