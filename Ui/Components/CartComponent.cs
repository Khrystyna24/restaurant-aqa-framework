using Microsoft.Playwright;

namespace Restaurant_Aqa_Framework.Ui.Components;

/// <summary>Header-mounted mini-cart panel (plain inline div, not a CDK overlay) — opened via
/// `HeaderComponent.OpenCartAsync`. Only the "not submitted" (draft) line items are modeled here,
/// matching the sample pre-order flow's scope; submitted/locked orders are out of scope.</summary>
public sealed class CartComponent(IPage page)
{
    private ILocator Root => page.Locator(".cart");

    public ILocator EmptyState => Root.Locator(".empty-cart");
    private ILocator DraftOrderItems => Root.Locator(".order-item-dish");
    public ILocator SubmitButton => Root.Locator(".submit-btn");
    public ILocator Total => Root.Locator(".total h2");

    public ILocator DishRow(string dishName) => DraftOrderItems.Filter(new() { HasText = dishName });

    public Task IncrementQuantityAsync(string dishName) =>
        DishRow(dishName).Locator(".quantity-buttons button").Nth(1).ClickAsync();

    public Task SubmitPreOrderAsync() => SubmitButton.ClickAsync();
    public Task CloseAsync() => Root.Locator(".close-icon").ClickAsync();
}
