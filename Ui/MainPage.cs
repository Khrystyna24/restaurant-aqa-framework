using Microsoft.Playwright;
using Restaurant_Aqa_Framework.Ui.Components;

namespace Restaurant_Aqa_Framework.Ui;

public sealed class MainPage(IPage page) : BasePage(page)
{
    public HeaderComponent Header { get; } = new(page);

    public ILocator BookTableCta => Page.GetByRole(AriaRole.Link, new() { Name = "Book a table" });
    public ILocator ViewMenuCta => Page.GetByRole(AriaRole.Link, new() { Name = "View Menu" });
    private ILocator LocationCards => Page.Locator(".location-cards > app-location-card");

    public Task NavigateAsync() => NavigateToAsync("/main");

    public LocationCardComponent LocationCard(string address) =>
        new(LocationCards.Filter(new() { HasText = address }));
}
