using Microsoft.Playwright;
using Restaurant_Aqa_Framework.Api.Dtos.Dishes;

namespace Restaurant_Aqa_Framework.Api;

/// <summary>POST /dishes has no gateway auth at all — <paramref name="bearerToken"/> is accepted purely
/// so a test can document that fact without special-casing the client.</summary>
public sealed class DishesApi(IAPIRequestContext api) : ApiClientBase(api)
{
    public Task<IAPIResponse> CreateDishAsync(CreateDishRequest request, string? bearerToken = null) =>
        Api.PostAsync(Url("/dishes"), new APIRequestContextOptions
        {
            DataString = Serialize(request),
            Headers = Headers(bearerToken),
        });

    public Task<IAPIResponse> GetDishesAsync(
        string? dishType = null,
        DishSortOrder? sortBy = null,
        string? name = null,
        string? locationId = null) =>
        GetAsync(Url("/dishes", new Dictionary<string, string?>
        {
            ["dishType"] = dishType,
            ["sortBy"] = sortBy?.ToQueryValue(),
            ["name"] = name,
            ["locationId"] = locationId,
        }));

    public Task<IAPIResponse> GetPopularDishesAsync() => GetAsync(Url("/dishes/popular"));

    public Task<IAPIResponse> GetDishAsync(string dishId) => GetAsync(Url($"/dishes/{dishId}"));

    /// <summary>DELETE /dishes/{id} now exists on the backend (confirmed live: a call with no
    /// <paramref name="bearerToken"/> returns 401 Unauthorized, not 404), so a caller must forward a
    /// waiter/admin token to actually delete anything. Confirmed live too: a dish still attached to
    /// any location's menu returns 400 unless <paramref name="force"/> is set, mirroring
    /// <see cref="LocationsApi.DeleteLocationAsync"/>'s force-cascade.</summary>
    public Task<IAPIResponse> DeleteDishAsync(string dishId, bool force = false, string? bearerToken = null) =>
        DeleteAsync(
            Url($"/dishes/{dishId}", new Dictionary<string, string?> { ["force"] = force ? "true" : null }),
            new APIRequestContextOptions { Headers = Headers(bearerToken) });
}
