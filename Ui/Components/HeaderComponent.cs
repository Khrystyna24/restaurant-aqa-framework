using Microsoft.Playwright;

namespace Restaurant_Aqa_Framework.Ui.Components;

/// <summary>Mounted on every `MainLayoutComponent` page, and hand-included on a few top-level pages
/// (location overview, reservation dashboards) — safe to construct from any page object's own `IPage`.</summary>
public sealed class HeaderComponent(IPage page)
{
    public ILocator SignInLink => page.GetByRole(AriaRole.Link, new() { Name = "Sign In" });
    public ILocator LogoutButton => page.GetByRole(AriaRole.Button, new() { Name = "Sign Out" });
    public ILocator CartToggleButton => page.Locator(".actions button").First;
    public ILocator MenuLink => page.GetByRole(AriaRole.Link, new() { Name = "Menu" });
    public ILocator BookTableLink => page.GetByRole(AriaRole.Link, new() { Name = "Book a Table" });

    /// <summary>Only present in the DOM at all once the dropdown is open (an `@if`, not just
    /// hidden by CSS) — call <see cref="OpenUserMenuAsync"/> first. Scoped to `.user-info` since
    /// "My Profile" and "Sign Out" inside the same dropdown also carry `.body-bold`.</summary>
    public ILocator SignedInUserLabel => page.Locator(".header-user-wrapper .user-info .body-bold");

    private ILocator UserDropdownTrigger => page.Locator(".header-user-wrapper");

    public Task OpenCartAsync() => CartToggleButton.ClickAsync();

    /// <summary>The dropdown (name/email, My Profile, Sign Out) is only rendered once the user
    /// wrapper is hovered/focused — hover it first or those elements don't exist yet.</summary>
    public Task OpenUserMenuAsync() => UserDropdownTrigger.HoverAsync();

    /// <summary>The desktop "Sign Out" button only exists inside a hover-triggered dropdown —
    /// hover the user wrapper first or the click target is detached/invisible.</summary>
    public async Task SignOutAsync()
    {
        await OpenUserMenuAsync();
        await LogoutButton.ClickAsync();
    }
}
