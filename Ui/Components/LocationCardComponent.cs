using Microsoft.Playwright;

namespace Restaurant_Aqa_Framework.Ui.Components;

/// <summary>`app-location-card`, shown in the main page's locations section — a link to
/// `/locations/{id}`. No rating/description here; those only appear on the detail page.</summary>
public sealed class LocationCardComponent(ILocator root)
{
    public ILocator Address => root.Locator(".location-text");
    public ILocator Capacity => root.Locator(".capacity .value");

    public Task OpenAsync() => root.ClickAsync();
}
