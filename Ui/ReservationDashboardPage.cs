using Microsoft.Playwright;
using Restaurant_Aqa_Framework.Ui.Components;

namespace Restaurant_Aqa_Framework.Ui;

public sealed class ReservationDashboardPage(IPage page) : BasePage(page)
{
    public HeaderComponent Header { get; } = new(page);
    public CartComponent Cart { get; } = new(page);

    private ILocator ReservationCards => Page.Locator("ul.reservation-list > li.card");
    public ILocator EmptyState => Page.Locator(".reservation-empty");

    public Task NavigateAsync() => NavigateToAsync("/reservations");

    public ReservationCardComponent Reservation(int index) => new(ReservationCards.Nth(index));

    public ReservationCardComponent ReservationByAddress(string address) =>
        new(ReservationCards.Filter(new() { HasText = address }));
}
