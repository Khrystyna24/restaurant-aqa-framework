using Microsoft.Playwright;

namespace Restaurant_Aqa_Framework.Ui.Components;

/// <summary>`app-table-card` on the `/tables` results list — shows up to 5 time slots inline, with
/// a "Show all" button opening the time-slots modal when there are more.</summary>
public sealed class TableCardComponent(ILocator root)
{
    public ILocator LocationName => root.Locator(".location-name");
    public ILocator ShowAllSlotsButton => root.Locator(".show-all-button");

    public ILocator TimeSlot(string label) => root.Locator("app-time-slot").Filter(new() { HasText = label });

    public Task SelectTimeSlotAsync(string label) => TimeSlot(label).ClickAsync();
}
