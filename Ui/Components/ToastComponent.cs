using Microsoft.Playwright;

namespace Restaurant_Aqa_Framework.Ui.Components;

/// <summary>The one genuine Angular Material/CDK element in the app (`MatSnackBar`) — it renders
/// into an overlay appended near &lt;body&gt;, so always construct this from the top-level `IPage`,
/// never scoped under a specific page object's own root locator. `panelClass` (toast-success /
/// toast-error / toast-info) is the stable hook, not Material's own structural classes.</summary>
public sealed class ToastComponent(IPage page)
{
    public ILocator Success => page.Locator(".toast-success .toast-message");
    public ILocator Error => page.Locator(".toast-error .toast-message");
    public ILocator Info => page.Locator(".toast-info .toast-message");
}
