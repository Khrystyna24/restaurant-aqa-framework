using Microsoft.Playwright;
using Restaurant_Aqa_Framework.Api;
using Restaurant_Aqa_Framework.Api.Dtos.Auth;
using Restaurant_Aqa_Framework.Api.Dtos.Dishes;
using Restaurant_Aqa_Framework.Api.Dtos.Feedback;
using Restaurant_Aqa_Framework.Api.Dtos.Locations;
using Restaurant_Aqa_Framework.Api.Dtos.Users;
using Restaurant_Aqa_Framework.Api.Seeding;
using Restaurant_Aqa_Framework.Configuration;

namespace Restaurant_Aqa_Framework.Hooks;

/// <summary>
/// Best-effort insurance sweep, independent of (and in addition to) the per-scenario
/// <see cref="CleanupScope"/>: a run that was killed, crashed, or had a cleanup step itself fail
/// leaves "AQA-Sample-"-prefixed records behind in the shared live environment. This hook looks
/// for every entity <see cref="SampleDataFactory"/> can create and deletes whatever it finds,
/// before the first scenario runs and again after the last one has finished.
///
/// As of this writing the backend has no dedicated dev/cleanup/bulk-delete endpoint (confirmed
/// against backend Routing/RouteRegistry.cs — no such route exists), so this reuses the regular
/// per-entity DELETE routes instead. Locations and Dishes both have a real DELETE (confirmed live:
/// a call with no bearer token returns 401 Unauthorized, not 404 — the route exists, it just needs a
/// waiter token, which this hook now forwards). Feedback and Users have no DELETE route at all yet
/// (same RouteRegistry.cs check), so those two sweeps can find stale records but cannot remove them;
/// every failure is logged and swallowed rather than failing the run.
///
/// Reservations/Orders are intentionally not swept: CreateReservationRequest/CreateOrderRequest
/// carry no free-text field, so they can never be tagged with the AQA-Sample- prefix in the first
/// place. Tables are not swept independently either — there is no endpoint to list them outside a
/// location — but ride along when a stray Location is force-deleted (confirmed live: force=true
/// cascades and clears attached Tables too, see LocationsApi.cs).
///
/// Deliberately does not touch <see cref="PlaywrightRunHooks"/>'s shared Browser/Playwright: the
/// Reqnroll.TUnit plugin wires [BeforeTestRun] bindings as native TUnit hooks that run in generator
/// emission order, not Reqnroll's declared Order (confirmed live - an Order=1 attempt here still ran
/// before PlaywrightRunHooks.StartBrowser and crashed on a null Playwright). No browser is needed for
/// pure API cleanup, so this hook spins up its own short-lived IPlaywright driver instead.
/// </summary>
[Binding]
public static class PreRunCleanupHooks
{
    private const string LogPrefix = "[PreRunCleanup]";

    // Only genuine failures (bad responses, exceptions) go to Console.Error - a sweep finding and
    // deleting leftover data is expected behavior, not a failure, and must not be reported as one.
    private static void LogInfo(string message) => Console.WriteLine($"{LogPrefix} {message}");

    // Leftover AQA-Sample- data found at sweep time means a previous run's per-scenario
    // CleanupScope didn't fully clean up - worth flagging, but it's not an error on its own.
    private static void LogWarn(string message) => Console.WriteLine($"{LogPrefix} WARN: {message}");

    private static void ReportSweep(string entity, int found, int deleted, int failed, string? note = null)
    {
        if (found == 0)
        {
            LogInfo($"{entity}: no leftovers found.");
            return;
        }

        LogWarn($"{entity}: {found} found, {deleted} deleted, {failed} failed{(note is null ? "." : $" ({note}).")}");
    }

    // The same sweep runs again once the run is over, so data left by scenarios that failed halfway (or
    // whose own cleanup failed) is removed on every run, green or red — not only by the next run's start.
    [BeforeTestRun]
    public static Task SweepStaleAqaSampleData() => SweepAsync();

    [AfterTestRun]
    public static Task SweepAqaSampleDataAfterRun() => SweepAsync();

    private static async Task SweepAsync()
    {
        if (string.Equals(Environment.GetEnvironmentVariable("SKIP_PRE_RUN_CLEANUP"), "true", StringComparison.OrdinalIgnoreCase))
        {
            LogInfo("Skipped (SKIP_PRE_RUN_CLEANUP=true).");
            return;
        }

        var settings = Settings.Current;

        // Reqnroll's [BeforeTestRun] Order isn't honored by the Reqnroll.TUnit plugin's native hook
        // wiring (confirmed live: this hook ran before PlaywrightRunHooks.StartBrowser regardless of
        // Order), so a browser/page is never needed here anyway - create our own short-lived
        // IPlaywright driver instance rather than depending on PlaywrightRunHooks.Playwright.
        using var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        var api = await playwright.APIRequest.NewContextAsync(
            new APIRequestNewContextOptions { BaseURL = settings.ApiBaseUrl });

        try
        {
            var waiterToken = await TrySignInAsync(api, settings.WaiterUsername, settings.WaiterPassword, "waiter");
            var adminToken = await TrySignInAsync(api, settings.AdminUsername, settings.AdminPassword, "admin");

            await SweepDishesAsync(api, waiterToken);
            await SweepLocationsAsync(api, waiterToken);
            await SweepUsersAsync(api, waiterToken, adminToken);

            LogInfo("Reservations/Orders: out of scope, no free-text field exists to carry the AQA-Sample- prefix.");
        }
        finally
        {
            await api.DisposeAsync();
        }
    }

    private static async Task<string?> TrySignInAsync(IAPIRequestContext api, string? username, string? password, string role)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            LogInfo($"No {role} credentials configured; steps requiring a {role} token will be skipped.");
            return null;
        }

        try
        {
            var authApi = new AuthApi(api);
            var response = await authApi.SignInAsync(new SignInRequest(username, password));
            if (!response.Ok)
            {
                Console.Error.WriteLine($"{LogPrefix} {role} sign-in returned {response.Status}; steps requiring a {role} token will be skipped.");
                return null;
            }

            var body = await response.ReadAsAsync<SignInResponse>();
            return body.IdToken;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"{LogPrefix} {role} sign-in threw: {ex.Message}; steps requiring a {role} token will be skipped.");
            return null;
        }
    }

    private static async Task SweepDishesAsync(IAPIRequestContext api, string? waiterToken)
    {
        var dishesApi = new DishesApi(api);
        var dishes = await TryFetchAsync<DishListResponse>(
            "dishes", () => dishesApi.GetDishesAsync(name: SampleDataFactory.Prefix));

        var (deleted, failed) = await DeleteAllAsync(
            dishes, d => $"dish:{d.Id}", d => dishesApi.DeleteDishAsync(d.Id, force: true, bearerToken: waiterToken));

        ReportSweep("Dishes", dishes.Count, deleted, failed);
    }

    private static async Task SweepLocationsAsync(IAPIRequestContext api, string? waiterToken)
    {
        var locationsApi = new LocationsApi(api);
        var allLocations = await TryFetchAsync<LocationResponse>("locations", locationsApi.GetLocationsAsync);
        var staleLocations = allLocations
            .Where(l => l.Address.StartsWith(SampleDataFactory.Prefix, StringComparison.Ordinal))
            .ToList();

        foreach (var location in staleLocations)
            await SweepFeedbackForLocationAsync(api, location.Id, waiterToken);

        var (deleted, failed) = await DeleteAllAsync(
            staleLocations,
            l => $"location:{l.Id}",
            l => locationsApi.DeleteLocationAsync(l.Id, force: true, bearerToken: waiterToken));

        ReportSweep(
            "Locations", staleLocations.Count, deleted, failed,
            "Tables have no listing/delete endpoint of their own and are expected to ride along via force-cascade");
    }

    private static async Task SweepFeedbackForLocationAsync(IAPIRequestContext api, string locationId, string? bearerToken)
    {
        var feedbackApi = new FeedbackApi(api);
        var staleFeedback = new List<FeedbackResponseItem>();

        foreach (var type in Enum.GetValues<FeedbackType>())
        {
            var page = 1;
            while (true)
            {
                var response = await TryFetchRawAsync(
                    "feedback", () => feedbackApi.GetFeedbackForLocationAsync(locationId, type, SortingOrder.NewestFirst, page));
                if (response is null)
                    break;

                var body = await response.ReadAsAsync<FeedbackResponse>();
                staleFeedback.AddRange(body.Items.Where(
                    f => f.Comment?.StartsWith(SampleDataFactory.Prefix, StringComparison.Ordinal) == true));

                if (page >= body.TotalPages)
                    break;
                page++;
            }
        }

        var (deleted, failed) = await DeleteAllAsync(
            staleFeedback,
            f => $"feedback:{f.ReservationId}@{f.CreatedAt:O}",
            f => feedbackApi.DeleteFeedbackAsync(f.ReservationId, f.CreatedAt, bearerToken));

        if (staleFeedback.Count > 0)
            LogWarn($"Feedback (location {locationId}): {staleFeedback.Count} found, {deleted} deleted, {failed} failed.");
    }

    private static async Task SweepUsersAsync(IAPIRequestContext api, string? waiterToken, string? adminToken)
    {
        if (waiterToken is null)
        {
            LogInfo("Users: skipped, GET /users requires a waiter token.");
            return;
        }

        var usersApi = new UsersApi(api);
        var users = await TryFetchAsync<UserDto>(
            "users", () => usersApi.SearchUsersByGivenNamePrefixAsync(SampleDataFactory.Prefix, waiterToken));

        if (adminToken is null)
        {
            LogWarn(
                $"Users: {users.Count} found but no admin token configured to delete them " +
                (users.Count > 0 ? $"({string.Join(", ", users.Select(u => u.Email))})." : "."));
            return;
        }

        var (deleted, failed) = await DeleteAllAsync(
            users, u => $"user:{u.Id}", u => usersApi.DeleteUserAsync(u.Id, adminToken));

        ReportSweep("Users", users.Count, deleted, failed);
    }

    private static async Task<List<T>> TryFetchAsync<T>(string label, Func<Task<IAPIResponse>> fetch)
    {
        var response = await TryFetchRawAsync(label, fetch);
        if (response is null)
            return [];

        try
        {
            return await response.ReadAsAsync<List<T>>();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"{LogPrefix} Failed to parse {label} response: {ex.Message}");
            return [];
        }
    }

    private static async Task<IAPIResponse?> TryFetchRawAsync(string label, Func<Task<IAPIResponse>> fetch)
    {
        try
        {
            var response = await fetch();
            if (!response.Ok)
            {
                var error = await response.ReadErrorMessageAsync();
                Console.Error.WriteLine($"{LogPrefix} Fetching {label} returned {response.Status}: {error}");
                return null;
            }

            return response;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"{LogPrefix} Fetching {label} threw: {ex.Message}");
            return null;
        }
    }

    private static async Task<(int Deleted, int Failed)> DeleteAllAsync<T>(
        IReadOnlyList<T> items, Func<T, string> label, Func<T, Task<IAPIResponse>> delete)
    {
        var deleted = 0;
        var failed = 0;

        foreach (var item in items)
        {
            try
            {
                var response = await delete(item);
                if (response.Ok)
                {
                    deleted++;
                }
                else
                {
                    failed++;
                    var error = await response.ReadErrorMessageAsync();
                    Console.Error.WriteLine($"{LogPrefix} Best-effort cleanup of '{label(item)}' returned {response.Status}: {error}");
                }
            }
            catch (Exception ex)
            {
                failed++;
                Console.Error.WriteLine($"{LogPrefix} Best-effort cleanup of '{label(item)}' threw: {ex.Message}");
            }
        }

        return (deleted, failed);
    }
}
