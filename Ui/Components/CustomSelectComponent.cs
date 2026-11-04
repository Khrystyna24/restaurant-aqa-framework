using Microsoft.Playwright;

namespace Restaurant_Aqa_Framework.Ui.Components;

/// <summary>The app's hand-rolled dropdown (`.custom-select`, role=listbox options) — used
/// everywhere a native &lt;select&gt; would normally go (location/date/time filters, make-reservation
/// time pickers, waiter table/location pickers). Not a real &lt;select&gt;, so `SelectOptionAsync`
/// does not apply: open it, then click the option by its visible text.</summary>
public sealed class CustomSelectComponent(ILocator root)
{
    private ILocator Dropdown => root.Locator(".dropdown");

    public async Task SelectAsync(string optionText)
    {
        await root.ClickAsync();
        await Dropdown.GetByRole(AriaRole.Option, new() { Name = optionText }).ClickAsync();
    }
}
