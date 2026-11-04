using Microsoft.Playwright;

namespace Restaurant_Aqa_Framework.Api;

public sealed class UsersApi(IAPIRequestContext api) : ApiClientBase(api)
{
    /// <summary>GET /users?fullName=... is Waiter-role-gated (Handlers/Users/GetUsersHandler.cs) and
    /// forwards its first whitespace-separated token straight into Cognito's <c>given_name ^= "..."</c>
    /// ("begins with") filter. Passing a single token with no space — e.g. SampleDataFactory.Prefix —
    /// turns this into a real prefix search across every stray AQA-created customer, regardless of the
    /// random first name Bogus appended after it.</summary>
    public Task<IAPIResponse> SearchUsersByGivenNamePrefixAsync(string prefix, string bearerToken) =>
        GetAsync(Url("/users", new Dictionary<string, string?> { ["fullName"] = prefix }),
            new APIRequestContextOptions { Headers = Headers(bearerToken) });

    /// <summary>DELETE /users/{id} does not exist on the backend yet (not in RouteRegistry.cs). Wired up
    /// best-effort, same pattern as DishesApi.DeleteDishAsync/TablesApi.DeleteTableAsync — expected to
    /// fail (404/405) until the backend ships an admin-gated hard-delete route for Cognito users.</summary>
    public Task<IAPIResponse> DeleteUserAsync(string id, string? bearerToken = null) =>
        DeleteAsync(Url($"/users/{id}"), new APIRequestContextOptions { Headers = Headers(bearerToken) });
}
