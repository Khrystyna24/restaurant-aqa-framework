using System.Globalization;
using Microsoft.Playwright;

namespace Restaurant_Aqa_Framework.Ui.Components;

/// <summary>The app's hand-rolled numeric stepper (`.counter` with two unlabeled buttons) — used
/// for guest counts everywhere. The buttons carry no accessible name, so they're located by
/// position: first is decrement, second is increment.</summary>
public sealed class CustomNumberComponent(ILocator root)
{
    private ILocator Buttons => root.Locator(".counter button");
    public ILocator Value => root.Locator(".counter .value");

    public Task IncrementAsync() => Buttons.Nth(1).ClickAsync();
    public Task DecrementAsync() => Buttons.Nth(0).ClickAsync();

    public async Task SetAsync(int target)
    {
        var current = int.Parse((await Value.TextContentAsync())!.Trim(), CultureInfo.InvariantCulture);
        while (current < target)
        {
            await IncrementAsync();
            current++;
        }

        while (current > target)
        {
            await DecrementAsync();
            current--;
        }
    }
}
