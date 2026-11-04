using Microsoft.Playwright;

namespace Restaurant_Aqa_Framework.Ui.Components;

/// <summary>Shown right after a successful booking — plain inline modal, not a CDK overlay.</summary>
public sealed class ReservationConfirmedModalComponent(IPage page)
{
    private ILocator Root => page.Locator(".modal-container").Filter(new() { Has = page.Locator(".confirmed-title") });

    public ILocator Title => Root.Locator(".confirmed-title");
    public ILocator Details => Root.Locator(".reservation-info p");
    public ILocator CloseButton => Root.Locator(".close-btn");
    public ILocator CancelReservationButton => Root.GetByRole(AriaRole.Button, new() { Name = "Cancel Reservation" });
    public ILocator EditReservationButton => Root.GetByRole(AriaRole.Button, new() { Name = "Edit Reservation" });
}
