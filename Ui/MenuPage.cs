using Microsoft.Playwright;
using Restaurant_Aqa_Framework.Ui.Components;

namespace Restaurant_Aqa_Framework.Ui;

public sealed class MenuPage(IPage page) : BasePage(page)
{
    public HeaderComponent Header { get; } = new(page);

    private ILocator CategoryButtons => Page.Locator(".category-buttons button");
    public CustomSelectComponent SortSelect => new(Page.Locator(".sort-filter"));
    private ILocator DishCards => Page.Locator("app-dish-card");

    public Task NavigateAsync() => NavigateToAsync("/menu");

    /// <summary>Pre-order mode is entered via query params, carrying the reservation the dishes
    /// will be attached to — there is no in-app link to it from the plain menu page.</summary>
    public Task NavigateForPreOrderAsync(string reservationId, string reservationLabel) =>
        NavigateToAsync($"/menu?preOrder=true&reservationId={Uri.EscapeDataString(reservationId)}&reservationLabel={Uri.EscapeDataString(reservationLabel)}");

    public Task FilterByCategoryAsync(string category) =>
        CategoryButtons.Filter(new() { HasText = category }).ClickAsync();

    public DishCardComponent Dish(string dishName) => new(DishCards.Filter(new() { HasText = dishName }));
}
