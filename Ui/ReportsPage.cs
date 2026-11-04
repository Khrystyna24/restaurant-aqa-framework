using Microsoft.Playwright;

namespace Restaurant_Aqa_Framework.Ui;

/// <summary>The admin-only reporting dashboard (`/reports`, behind the frontend's `AdminGuard`).</summary>
public sealed class ReportsPage(IPage page) : BasePage(page)
{
    public ILocator Dashboard => Page.Locator("app-reporting-dashboard");

    public Task NavigateAsync() => NavigateToAsync("/reports");
}
