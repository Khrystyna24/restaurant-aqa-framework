using Microsoft.Playwright;
using Restaurant_Aqa_Framework.Api.Dtos.Orders;

namespace Restaurant_Aqa_Framework.Api;

public sealed class OrdersApi(IAPIRequestContext api) : ApiClientBase(api)
{
    public Task<IAPIResponse> GetOrdersAsync(string? bearerToken = null) =>
        GetAsync(Url("/orders"), new APIRequestContextOptions { Headers = Headers(bearerToken) });

    public Task<IAPIResponse> GetOrderAsync(string reservationId, string? bearerToken = null) =>
        GetAsync(Url($"/orders/{reservationId}"), new APIRequestContextOptions { Headers = Headers(bearerToken) });

    public Task<IAPIResponse> CreateOrderAsync(CreateOrderRequest request, string? bearerToken = null) =>
        Api.PostAsync(Url("/orders"), new APIRequestContextOptions
        {
            DataString = Serialize(request),
            Headers = Headers(bearerToken),
        });

    public Task<IAPIResponse> EditOrderAsync(
        string reservationId, EditPreOrderRequest request, string? bearerToken = null) =>
        PutAsync(Url($"/orders/{reservationId}"), new APIRequestContextOptions
        {
            DataString = Serialize(request),
            Headers = Headers(bearerToken),
        });

    public Task<IAPIResponse> DeleteOrderAsync(string reservationId, string? bearerToken = null) =>
        DeleteAsync(Url($"/orders/{reservationId}"), new APIRequestContextOptions
        {
            Headers = Headers(bearerToken),
        });

    public Task<IAPIResponse> SubmitOrderAsync(string reservationId, string? bearerToken = null) =>
        Api.PostAsync(Url($"/orders/{reservationId}/submit"), new APIRequestContextOptions
        {
            Headers = Headers(bearerToken),
        });

    public Task<IAPIResponse> UnsubmitOrderAsync(string reservationId, string? bearerToken = null) =>
        Api.PostAsync(Url($"/orders/{reservationId}/unsubmit"), new APIRequestContextOptions
        {
            Headers = Headers(bearerToken),
        });
}
