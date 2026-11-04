using Microsoft.Playwright;

namespace Restaurant_Aqa_Framework.Ui.Components;

/// <summary>`app-dish-card` — on the menu page it renders as a clickable `role="button"` div
/// (opens the detail modal) rather than a link; the "Pre-order" action button only appears when
/// the page was reached in pre-order mode (`/menu?preOrder=true&amp;...`).</summary>
public sealed class DishCardComponent(ILocator root)
{
    public ILocator Name => root.Locator(".dish-name");
    public ILocator Price => root.Locator(".dish-price");
    public ILocator StatusBadge => root.Locator(".status-badge");
    public ILocator PreOrderButton => root.GetByRole(AriaRole.Button, new() { Name = "Pre-order" });

    public Task OpenDetailsAsync() => root.ClickAsync();
    public Task PreOrderAsync() => PreOrderButton.ClickAsync();
}
