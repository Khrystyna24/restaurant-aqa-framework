using Restaurant_Aqa_Framework.Api;
using Restaurant_Aqa_Framework.Api.Dtos.Locations;
using Restaurant_Aqa_Framework.Api.Dtos.Tables;
using Restaurant_Aqa_Framework.Api.Seeding;
using Restaurant_Aqa_Framework.Screenplay.Questions;

namespace Restaurant_Aqa_Framework.Screenplay.Tasks;

/// <summary>Adds a sample table (of the given size, or any) to a location: the one named, or else the one this
/// scenario set up most recently.</summary>
public sealed class AddTable(int? seats = null, string? toLocation = null) : ITask
{
    public async Task PerformAsAsync(Actor actor)
    {
        var location = actor.Recall<LocationResponse>(toLocation);
        var api = new TablesApi(actor.UsingTheApi());
        var response = actor.TheLastResponse();

        await response.CaptureAsync(await api.CreateTableAsync(SampleDataFactory.CreateTableRequest(location.Id, seats)));
        if (!response.Succeeded)
            throw new InvalidOperationException($"Adding the table failed with {response.Status}: {response.Body}");

        var table = response.Json<TableResponse>();
        actor.Remember(table);

        var waiter = await actor.AsksForAsync(new TheWaiterSession());
        actor.Recall<CleanupScope>().TrackDelete(
            $"table:{table.Id}",
            () => api.DeleteTableAsync(table.Id, bearerToken: waiter.IdToken));
    }
}
