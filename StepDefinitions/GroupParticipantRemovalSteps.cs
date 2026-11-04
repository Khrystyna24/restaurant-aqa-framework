using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Reqnroll;
using Restaurant_Aqa_Framework.Api;
using Restaurant_Aqa_Framework.Api.Dtos.Dishes;
using Restaurant_Aqa_Framework.Api.Seeding;
using Restaurant_Aqa_Framework.Hooks;

namespace Restaurant_Aqa_Framework.StepDefinitions;

public sealed class GroupState
{
    private string? _apiUrl;
    private string? _uiUrl;

    public string ApiUrl => _apiUrl ??= Require("API_URL");
    public string UiUrl => _uiUrl ??= Require("UI_URL");

    public string Prefix { get; } = "AQA-Sample-";

    public string TableId { get; set; } = "";

    public CleanupScope Scope { get; } = new();

    public IAPIRequestContext? ApiCtx { get; set; }

    public GroupApiClient.OwnerAccount Owner { get; set; } = null!;
    public GroupApiClient.OwnerAccount SecondOwner { get; set; } = null!;
    public string ReservationId { get; set; } = "";
    public string GroupId { get; set; } = "";
    public string SecondGroupId { get; set; } = "";
    public List<GroupApiClient.Dish> Dishes { get; set; } = new();

    public Dictionary<string, GuestInfo> Guests { get; } = new();
    public HashSet<string> Removed { get; } = new();

    public GroupApiClient.GuestSession? SecondGroupGuest { get; set; }
    public ApiResultHolder Last { get; } = new();

    public List<(string ReservationId, string Token)> ToCleanup { get; } = new();

    public static string? Get(string name)
    {
        var fromEnv = Environment.GetEnvironmentVariable(name);
        if (!string.IsNullOrWhiteSpace(fromEnv))
        {
            return fromEnv;
        }

        return DotEnv.Value.TryGetValue(name, out var v) && !string.IsNullOrWhiteSpace(v) ? v : null;
    }

    private static string Require(string name)
        => Get(name) is { Length: > 0 } v
            ? v
            : throw new InvalidOperationException($"{name} is not set (.env / environment).");

    private static readonly Lazy<Dictionary<string, string>> DotEnv = new(() =>
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        for (var i = 0; i < 6 && dir is not null; i++, dir = dir.Parent)
        {
            var path = Path.Combine(dir.FullName, ".env");
            if (!File.Exists(path))
            {
                continue;
            }

            foreach (var raw in File.ReadAllLines(path))
            {
                var line = raw.Trim();
                if (line.Length == 0 || line.StartsWith('#') || !line.Contains('=', StringComparison.Ordinal))
                {
                    continue;
                }

                var idx = line.IndexOf('=', StringComparison.Ordinal);
                result[line[..idx].Trim()] = line[(idx + 1)..].Trim().Trim('"');
            }

            break;
        }

        return result;
    });
}

public sealed record GuestInfo(GroupApiClient.GuestSession Session, decimal Total, bool IsSubmitted);

public sealed class ApiResultHolder
{
    public HttpStatusCode Status { get; set; }
    public string Body { get; set; } = "";
}

[Binding]
public sealed class GroupParticipantRemovalSteps
{
    private static readonly Random Rng = new();
    private readonly GroupState _s;
    private readonly ScenarioContext _ctx;
    private IPage? _page;

    public GroupParticipantRemovalSteps(GroupState state, ScenarioContext ctx)
    {
        _s = state;
        _ctx = ctx;
    }

    private async Task<IPage> GetPageAsync()
    {
        if (_page != null) return _page;

        if (_ctx.ScenarioContainer.IsRegistered<IPage>())
        {
            _page = _ctx.ScenarioContainer.Resolve<IPage>();
            return _page;
        }

        var channel = GroupState.Get("CHANNEL") ?? "chrome";
        var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions 
        { 
            Headless = true,
            Channel = channel
        });
        var context = await browser.NewContextAsync();
        _page = await context.NewPageAsync();
        _ctx.ScenarioContainer.RegisterInstanceAs(_page);
        return _page;
    }

    // =================================================================== GIVEN
    [Given("the Owner has a group reservation")]
    public async Task GivenOwnerHasGroup()
    {
        var res = await CreateGroupAsync("owner");
        _s.Owner = res.Owner;
        _s.ReservationId = res.ReservationId;
        _s.GroupId = res.GroupId;
    }

    [Given("the guest {string} joined and submitted a cart with {int} x dish {int}")]
    public async Task GivenGuestSubmitted(string name, int qty, int dishNo)
        => await JoinAsync(name, qty, dishNo, submit: true);

    [Given("the guest {string} joined with an unsubmitted cart with {int} x dish {int}")]
    public async Task GivenGuestUnsubmitted(string name, int qty, int dishNo)
        => await JoinAsync(name, qty, dishNo, submit: false);

    [Given("the guest {string} submitted the cart")]
    public async Task GivenGuestSubmittedLater(string name)
    {
        using var api = new GroupApiClient(_s.ApiUrl);
        await api.SubmitCartAsync(_s.Guests[name].Session);

        var info = _s.Guests[name];
        _s.Guests[name] = info with { IsSubmitted = true };
    }

    [Given("a second independent group with the guest {string}")]
    public async Task GivenSecondGroup(string name)
    {
        var res = await CreateGroupAsync("owner2");
        _s.SecondOwner = res.Owner;
        _s.SecondGroupId = res.GroupId;
        using var api = new GroupApiClient(_s.ApiUrl);
        _s.SecondGroupGuest = await api.CreateGuestSessionAsync(res.GroupId, name);
    }

    // ============================================================== WHEN: UI
    [When("the Owner opens the group order page")]
    public async Task WhenOwnerOpensPage()
    {
        var page = await GetPageAsync();
        var auth = JsonSerializer.Serialize(new { idToken = _s.Owner.IdToken, accessToken = _s.Owner.IdToken });
        var user = JsonSerializer.Serialize(new
        {
            id = _s.Owner.UserId,
            firstName = "Aqa",
            lastName = "Tester",
            email = _s.Owner.Email,
            role = "Customer",
        });

        await page.GotoAsync(_s.UiUrl);
        await page.EvaluateAsync(
            "([a, u]) => { localStorage.setItem('auth', a); localStorage.setItem('user', u); }",
            new[] { auth, user });
        await page.GotoAsync($"{_s.UiUrl.TrimEnd('/')}/reservations/{_s.ReservationId}/group-order");
        await Assertions.Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Group Pre-Order" })).ToBeVisibleAsync();
    }

    [When("the Owner removes {string} from the group")]
    public async Task WhenOwnerRemovesUi(string name)
    {
        var page = await GetPageAsync();
        var button = Card(page, name).GetByRole(AriaRole.Button, new() { Name = "Remove", Exact = true });
        await Assertions.Expect(button).ToBeVisibleAsync();
        await button.ClickAsync();
        _s.Removed.Add(name);
    }

    [When("the Owner submits the consolidated pre-order")]
    public async Task WhenOwnerSubmits()
    {
        var page = await GetPageAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Submit Pre-Order" }).ClickAsync();
        
        await page.RunAndWaitForResponseAsync(
            () => page.Locator("app-modal").GetByRole(AriaRole.Button, new() { Name = "Submit", Exact = true }).ClickAsync(),
            resp => resp.Url.Contains("/submit") && (int)resp.Status >= 200 && (int)resp.Status < 300
        );
    }

    // ============================================================= WHEN: API
    [When("the Owner removes {string} via API")]
    public async Task WhenOwnerRemovesApi(string name)
    {
        using var api = new GroupApiClient(_s.ApiUrl);
        var r = await api.RemoveSessionAsync(_s.GroupId, _s.Guests[name].Session.SessionId, _s.Owner.IdToken);
        if ((int)r.Status is < 200 or >= 300)
        {
            throw new InvalidOperationException($"Owner could not remove {name}: {(int)r.Status} {r.Body}");
        }

        _s.Removed.Add(name);
    }

    [When("the Owner removes {string} through the first group via API")]
    public async Task WhenRemovesForeign(string name)
    {
        using var api = new GroupApiClient(_s.ApiUrl);
        await Store(api.RemoveSessionAsync(_s.GroupId, _s.SecondGroupGuest!.SessionId, _s.Owner.IdToken));
    }

    [When("the Owner removes a non-existent participant via API")]
    public async Task WhenRemovesMissing()
    {
        using var api = new GroupApiClient(_s.ApiUrl);
        await Store(api.RemoveSessionAsync(_s.GroupId, Guid.NewGuid().ToString(), _s.Owner.IdToken));
    }

    [When("a guest of this group tries to remove {string} via API")]
    public async Task WhenGuestTries(string name)
    {
        using var api = new GroupApiClient(_s.ApiUrl);
        var guestCaller = _s.Guests.Values.First(g => g.Session.SessionId != _s.Guests[name].Session.SessionId);
        await Store(api.RemoveSessionAsync(
            _s.GroupId, _s.Guests[name].Session.SessionId, guestCaller.Session.SessionToken));
    }

    [When("another customer tries to remove {string} via API")]
    public async Task WhenOtherCustomerTries(string name)
    {
        var other = await GroupApiClient.RegisterAndSignInAsync(_s.ApiUrl, _s.Prefix + "stranger");
        using var api = new GroupApiClient(_s.ApiUrl);
        await Store(api.RemoveSessionAsync(_s.GroupId, _s.Guests[name].Session.SessionId, other.IdToken));
    }

    [When("an unauthenticated user tries to remove {string} via API")]
    public async Task WhenAnonymousTries(string name)
    {
        using var api = new GroupApiClient(_s.ApiUrl);
        await Store(api.RemoveSessionAsync(_s.GroupId, _s.Guests[name].Session.SessionId, null));
    }

    // ================================================================== THEN
    [Then("{string} no longer appears in the participants list")]
    public async Task ThenGone(string name)
    {
        var page = await GetPageAsync();
        await Assertions.Expect(Card(page, name)).ToHaveCountAsync(0);
    }

    [Then("{string} still appears in the participants list")]
    public async Task ThenStillThere(string name)
    {
        var page = await GetPageAsync();
        await Assertions.Expect(Card(page, name)).ToHaveCountAsync(1);
    }

    [Then("the grand total equals the sum of the remaining carts")]
    public async Task ThenTotalUi()
    {
        var page = await GetPageAsync();
        var expected = ExpectedTotal();
        await Assertions.Expect(page.Locator(".consolidated"))
            .ToContainTextAsync($"Grand Total: ${expected.ToString("N2", CultureInfo.InvariantCulture)}");

        using var api = new GroupApiClient(_s.ApiUrl);
        var view = await api.GetSessionsAsync(_s.GroupId, _s.Owner.IdToken);
        if (view.Total != expected)
        {
            throw new InvalidOperationException($"API total {view.Total} differs from expected {expected}");
        }
    }

    [Then("no guests remain in the group and the total is 0")]
    public async Task ThenEmpty()
    {
        using var api = new GroupApiClient(_s.ApiUrl);
        var view = await api.GetSessionsAsync(_s.GroupId, _s.Owner.IdToken);
        var guests = view.Participants.Where(p => p.Type == "Guest").ToList();
        if (guests.Count != 0 || view.Total != 0)
        {
            throw new InvalidOperationException(
                $"Expected no guests and total 0, got {guests.Count} guests, total {view.Total}");
        }
    }

    [Then("the API returns {int}")]
    public void ThenStatus(int code)
    {
        if ((int)_s.Last.Status != code)
        {
            throw new InvalidOperationException($"Expected {code}, got {(int)_s.Last.Status}: {_s.Last.Body}");
        }
    }

    [Then("{string} is still in the group")]
    public async Task ThenStillInGroup(string name)
    {
        using var api = new GroupApiClient(_s.ApiUrl);
        var view = await api.GetSessionsAsync(_s.GroupId, _s.Owner.IdToken);
        if (view.Participants.All(p => p.Id != _s.Guests[name].Session.SessionId))
        {
            throw new InvalidOperationException($"{name} is missing from the group");
        }
    }

    [Then("{string} is still in the second group")]
    public async Task ThenStillInSecond(string name)
    {
        using var api = new GroupApiClient(_s.ApiUrl);
        var view = await api.GetSessionsAsync(_s.SecondGroupId, _s.SecondOwner.IdToken);
        if (view.Participants.All(p => p.Id != _s.SecondGroupGuest!.SessionId))
        {
            throw new InvalidOperationException($"{name} is missing from the second group");
        }
    }

    [Then("the page shows {string}")]
    public async Task ThenPageShows(string text)
    {
        var page = await GetPageAsync();
        await Assertions.Expect(page.Locator(".status-badge")).ToHaveTextAsync(text);
    }

    [Then("the group status is {string}")]
    public async Task ThenGroupStatus(string status)
    {
        using var api = new GroupApiClient(_s.ApiUrl);
        var actual = await api.GetStatusAsync(_s.GroupId, _s.Owner.IdToken);
        if (actual != status)
        {
            throw new InvalidOperationException($"Expected status {status}, got {actual}");
        }
    }

    [Then("the waiter sees the order with dish {int} but without dish {int}")]
    public async Task ThenWaiterSees(int present, int absent)
    {
        var user = GroupState.Get("WAITER_USERNAME");
        var pass = GroupState.Get("WAITER_PASSWORD");
        if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
        {
            throw new InvalidOperationException(
                "WAITER_USERNAME/WAITER_PASSWORD are not set: this scenario needs a waiter account.");
        }

        var token = await GroupApiClient.SignInAsync(_s.ApiUrl, user, pass);
        using var api = new GroupApiClient(_s.ApiUrl);
        var order = await api.GetOrderAsync(token, _s.ReservationId);
        if ((int)order.Status is < 200 or >= 300)
        {
            throw new InvalidOperationException($"Waiter could not read the order: {(int)order.Status} {order.Body}");
        }

        var wanted = _s.Dishes[present - 1].Name;
        var unwanted = _s.Dishes[absent - 1].Name;
        if (!order.Body.Contains(wanted, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Order does not contain '{wanted}': {order.Body}");
        }

        if (order.Body.Contains(unwanted, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Order still contains '{unwanted}': {order.Body}");
        }
    }

    // =================================================== ЗАСІВ / ПІДГОТОВКА / ПРИБИРАННЯ
    private async Task SeedAsync()
    {
        if (_s.TableId.Length > 0)
        {
            return;
        }

        var user = GroupState.Get("WAITER_USERNAME");
        var pass = GroupState.Get("WAITER_PASSWORD");
        if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
        {
            throw new InvalidOperationException(
                "WAITER_USERNAME/WAITER_PASSWORD are not set: they are needed to create a table and dishes.");
        }

        var token = await GroupApiClient.SignInAsync(_s.ApiUrl, user, pass);
        var ctx = await PlaywrightRunHooks.Playwright.APIRequest.NewContextAsync();
        _s.ApiCtx = ctx;

        var tables = new TablesApi(ctx);
        var dishes = new DishesApi(ctx);

        var locationId = GroupState.Get("LOCATION_ID");
        if (string.IsNullOrWhiteSpace(locationId))
        {
            throw new InvalidOperationException("LOCATION_ID is not set in .env. Provide an existing location ID with a waiter.");
        }

        _s.TableId = await ReadIdAsync(
            await tables.CreateTableAsync(SampleDataFactory.CreateTableRequest(locationId, 10), token),
            "create table");
        _s.Scope.TrackDelete("table", () => tables.DeleteTableAsync(_s.TableId, bearerToken: token));

        for (var i = 0; i < 2; i++)
        {
            var dishId = await ReadIdAsync(
                await dishes.CreateDishAsync(
                    SampleDataFactory.CreateDishRequest(new[] { locationId }, DishType.MainCourse), token),
                "create dish");
            _s.Scope.TrackDelete(
                $"dish {i + 1}", () => dishes.DeleteDishAsync(dishId, force: true, bearerToken: token));
        }
    }

    private static async Task<string> ReadIdAsync(IAPIResponse response, string what)
    {
        var body = await response.TextAsync();
        if (!response.Ok)
        {
            throw new InvalidOperationException($"{what} failed: {response.Status} {body}");
        }

        using var doc = JsonDocument.Parse(body);
        foreach (var name in new[] { "id", "Id" })
        {
            if (doc.RootElement.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String)
            {
                return value.GetString()!;
            }
        }

        throw new InvalidOperationException($"{what}: id not found in {body}");
    }

    private async Task<(GroupApiClient.OwnerAccount Owner, string ReservationId, string GroupId)> CreateGroupAsync(string role)
    {
        await SeedAsync();
        var owner = await GroupApiClient.RegisterAndSignInAsync(_s.ApiUrl, _s.Prefix + role);
        using var api = new GroupApiClient(_s.ApiUrl);

        var start = DateTime.UtcNow.Date.AddDays(Rng.Next(30, 330)).AddHours(Rng.Next(9, 21));
        var reservationId = await api.CreateReservationAsync(owner.IdToken, _s.TableId, start, start.AddHours(1), 6);
        _s.ToCleanup.Add((reservationId, owner.IdToken));

        var invite = await api.GetInviteAsync(owner.IdToken, reservationId);
        return (owner, reservationId, invite.GroupId);
    }

    private async Task JoinAsync(string name, int qty, int dishNo, bool submit)
    {
        using var api = new GroupApiClient(_s.ApiUrl);
        var session = await api.CreateGuestSessionAsync(_s.GroupId, name);

        var selectableDishes = await api.GetSelectableDishesAsync(session);
        if (_s.Dishes.Count == 0)
        {
            _s.Dishes = selectableDishes;
        }

        if (selectableDishes.Count < dishNo)
        {
            throw new InvalidOperationException($"Menu has only {selectableDishes.Count} selectable dishes, need #{dishNo}");
        }

        var dish = selectableDishes[dishNo - 1];
        await api.AddDishAsync(session, dish.Id, qty);
        if (submit)
        {
            await api.SubmitCartAsync(session);
        }

        _s.Guests[name] = new GuestInfo(session, dish.Price * qty, submit);
    }

    private decimal ExpectedTotal()
        => _s.Guests
            .Where(g => !_s.Removed.Contains(g.Key) && g.Value.IsSubmitted)
            .Sum(g => g.Value.Total);

    private static ILocator Card(IPage page, string name)
        => page.Locator("app-participant-card").Filter(new() { HasText = name });

    private async Task Store(Task<GroupApiClient.ApiResult> call)
    {
        var r = await call;
        _s.Last.Status = r.Status;
        _s.Last.Body = r.Body;
    }

    [AfterScenario("GroupPreOrder")]
    public async Task Cleanup()
    {
        try
        {
            if (_s.ToCleanup.Count > 0)
            {
                await CancelReservationsAsync();
            }
        }
        finally
        {
            await _s.Scope.DisposeAsync();
            if (_s.ApiCtx is not null)
            {
                await _s.ApiCtx.DisposeAsync();
            }
            if (_page != null)
            {
                await _page.Context.CloseAsync();
            }
        }
    }

    private async Task CancelReservationsAsync()
    {
        using var api = new GroupApiClient(_s.ApiUrl);
        foreach (var (reservationId, token) in _s.ToCleanup)
        {
            try
            {
                await api.CancelReservationAsync(token, reservationId);
            }
            catch (Exception)
            {
                // Isolation
            }
        }
    }
}