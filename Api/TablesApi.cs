using System.Globalization;
using Microsoft.Playwright;
using Restaurant_Aqa_Framework.Api.Dtos.Tables;

namespace Restaurant_Aqa_Framework.Api;

/// <summary>Tables can be created and deleted, but not listed outside their location — cleanup helpers that
/// target a table should treat a failed delete as best-effort, since deleting the location with
/// <c>force=true</c> removes its tables anyway.</summary>
public sealed class TablesApi(IAPIRequestContext api) : ApiClientBase(api)
{
    public Task<IAPIResponse> CreateTableAsync(CreateTableRequest request, string? bearerToken = null) =>
        Api.PostAsync(Url("/tables"), new APIRequestContextOptions
        {
            DataString = Serialize(request),
            Headers = Headers(bearerToken),
        });

    public Task<IAPIResponse> GetAvailableTablesAsync(string locationId, int capacity, DateTime startTime) =>
        GetAsync(Url("/available-tables", new Dictionary<string, string?>
        {
            ["locationId"] = locationId,
            ["capacity"] = capacity.ToString(CultureInfo.InvariantCulture),
            ["startTime"] = startTime.ToString("O"),
        }));

    public Task<IAPIResponse> GetAvailableTimeSlotsAsync(string tableId, DateTime date) =>
        GetAsync(Url($"/tables/{tableId}/available-time-slots", new Dictionary<string, string?>
        {
            ["date"] = date.ToString("O"),
        }));

    /// <summary>DELETE /tables/{id} now exists on the backend (confirmed live: a call with no
    /// <paramref name="bearerToken"/> returns 401 Unauthorized, not 404), so a caller must forward a
    /// waiter/admin token to actually delete anything.</summary>
    public Task<IAPIResponse> DeleteTableAsync(string tableId, string? bearerToken = null) =>
        DeleteAsync(Url($"/tables/{tableId}"), new APIRequestContextOptions { Headers = Headers(bearerToken) });
}
