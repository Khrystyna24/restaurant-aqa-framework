using Microsoft.Playwright;
using Restaurant_Aqa_Framework.Api.Dtos.Locations;

namespace Restaurant_Aqa_Framework.Api;

/// <summary>Locations now has full CRUD (PUT/DELETE added recently) — unlike Dishes/Tables, DELETE
/// here is real. Confirmed live: it requires a bearer token (401 without one) and, once a table has
/// been created against the location, requires <c>force=true</c> too (400 otherwise, since Tables
/// have no delete endpoint of their own to clear them first). Seeding helpers must pass both.</summary>
public sealed class LocationsApi(IAPIRequestContext api) : ApiClientBase(api)
{
    public Task<IAPIResponse> CreateLocationAsync(CreateLocationRequest request, string? bearerToken = null) =>
        Api.PostAsync(Url("/locations"), new APIRequestContextOptions
        {
            DataString = Serialize(request),
            Headers = Headers(bearerToken),
        });

    public Task<IAPIResponse> GetLocationsAsync() => GetAsync(Url("/locations"));

    public Task<IAPIResponse> GetLocationAsync(string locationId) => GetAsync(Url($"/locations/{locationId}"));

    public Task<IAPIResponse> EditLocationAsync(
        string locationId, CreateLocationRequest request, string? bearerToken = null) =>
        PutAsync(Url($"/locations/{locationId}"), new APIRequestContextOptions
        {
            DataString = Serialize(request),
            Headers = Headers(bearerToken),
        });

    public Task<IAPIResponse> DeleteLocationAsync(string locationId, bool force = false, string? bearerToken = null) =>
        DeleteAsync(
            Url($"/locations/{locationId}", new Dictionary<string, string?> { ["force"] = force ? "true" : null }),
            new APIRequestContextOptions { Headers = Headers(bearerToken) });

    public Task<IAPIResponse> GetDishesForLocationAsync(string locationId) =>
        GetAsync(Url($"/locations/{locationId}/dishes"));
}
