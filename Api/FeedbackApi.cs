using System.Globalization;
using Microsoft.Playwright;
using Restaurant_Aqa_Framework.Api.Dtos.Feedback;

namespace Restaurant_Aqa_Framework.Api;

public sealed class FeedbackApi(IAPIRequestContext api) : ApiClientBase(api)
{
    public Task<IAPIResponse> GetFeedbackForLocationAsync(
        string locationId, FeedbackType type, SortingOrder sort, int page) =>
        GetAsync(Url("/feedback", new Dictionary<string, string?>
        {
            ["locationId"] = locationId,
            ["type"] = type.ToString(),
            ["sort"] = sort.ToString(),
            ["page"] = page.ToString(CultureInfo.InvariantCulture),
        }));

    public Task<IAPIResponse> CreateFeedbackAsync(CreateFeedbackRequest request, string? bearerToken = null) =>
        Api.PostAsync(Url("/feedback"), new APIRequestContextOptions
        {
            DataString = Serialize(request),
            Headers = Headers(bearerToken),
        });

    public Task<IAPIResponse> EditFeedbackAsync(CreateFeedbackRequest request, string? bearerToken = null) =>
        PutAsync(Url("/feedback"), new APIRequestContextOptions
        {
            DataString = Serialize(request),
            Headers = Headers(bearerToken),
        });

    /// <summary>DELETE /feedback/{reservationId} does not exist on the backend yet (not in
    /// RouteRegistry.cs — Handlers/Feedbacks/ has no delete code at all). Feedback's key is
    /// reservationId+createdAt, not a synthetic id, so createdAt is carried as a query param; the
    /// path shape otherwise mirrors OrdersApi.DeleteOrderAsync's /orders/{reservationId}. Wired up
    /// best-effort, same pattern as DishesApi.DeleteDishAsync — expected to fail (404/405) until the
    /// backend ships this route.</summary>
    public Task<IAPIResponse> DeleteFeedbackAsync(
        string reservationId, DateTime createdAt, string? bearerToken = null) =>
        DeleteAsync(
            Url($"/feedback/{reservationId}", new Dictionary<string, string?> { ["createdAt"] = createdAt.ToString("O") }),
            new APIRequestContextOptions { Headers = Headers(bearerToken) });
}
