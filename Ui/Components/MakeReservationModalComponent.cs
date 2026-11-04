using Microsoft.Playwright;

namespace Restaurant_Aqa_Framework.Ui.Components;

/// <summary>Rendered inline (plain `.modal-backdrop`/`.modal-container`, not a CDK overlay), so
/// ordinary page-scoped locators work. Wraps `MakeReservationComponent`, reused for both
/// "Make a Reservation" and "Edit Reservation" — the title and the confirm button's accessible
/// name both switch with it, so callers pass whichever is showing.</summary>
public sealed class MakeReservationModalComponent(IPage page)
{
    private ILocator Root => page.Locator(".modal-container").Filter(new() { Has = page.Locator(".guests-filter") });

    public CustomNumberComponent Guests => new(Root.Locator(".guests-filter"));

    private ILocator TimeSelectors => Root.Locator(".time-selectors .time-selector");
    public CustomSelectComponent FromTime => new(TimeSelectors.Nth(0));
    public CustomSelectComponent ToTime => new(TimeSelectors.Nth(1));

    public ILocator CloseButton => Root.Locator(".close-btn");

    public Task ConfirmAsync(string title = "Make a Reservation") =>
        Root.GetByRole(AriaRole.Button, new() { Name = title }).ClickAsync();
}
