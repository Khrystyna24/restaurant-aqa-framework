using Microsoft.Playwright;
using Restaurant_Aqa_Framework.Ui.Components;

namespace Restaurant_Aqa_Framework.Ui;

public sealed class TablesPage(IPage page) : BasePage(page)
{
    public HeaderComponent Header { get; } = new(page);
    public MakeReservationModalComponent MakeReservationModal { get; } = new(page);
    public ReservationConfirmedModalComponent ReservationConfirmed { get; } = new(page);

    public CustomSelectComponent LocationFilter => new(Page.Locator(".location-filter"));
    private ILocator GenericFilters => Page.Locator(".filter.custom-select");
    public CustomSelectComponent DateFilter => new(GenericFilters.Nth(0));
    public CustomSelectComponent TimeFilter => new(GenericFilters.Nth(1));
    public CustomNumberComponent GuestsFilter => new(Page.Locator(".guests-filter"));
    public ILocator FindTableButton => Page.Locator("button.find-button");

    private ILocator TableCards => Page.Locator(".table-card-list > app-table-card");

    public Task NavigateAsync() => NavigateToAsync("/tables");

    public Task NavigateWithLocationFilterAsync(string locationId) => NavigateToAsync($"/tables?locationId={locationId}");

    public Task FindTablesAsync() => FindTableButton.ClickAsync();

    public TableCardComponent Table(int index) => new(TableCards.Nth(index));

    public TableCardComponent TableByNumber(int tableNumber) =>
        new(TableCards.Filter(new() { HasText = $"Table {tableNumber}" }));
}
