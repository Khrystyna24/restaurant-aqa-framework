using Restaurant_Aqa_Framework.Api;
using Restaurant_Aqa_Framework.Api.Dtos.Locations;
using Restaurant_Aqa_Framework.Screenplay.Questions;

namespace Restaurant_Aqa_Framework.Screenplay.Tasks;

/// <summary>Edits a location (the named one, or else the most recent). The edit replaces the whole record, so
/// everything not being changed is sent back as it was (occupancy comes back as "70%" but is sent as "70").</summary>
public sealed class ChangeLocation(Func<LocationResponse, LocationResponse> change, string? location = null) : ITask
{
    public async Task PerformAsAsync(Actor actor)
    {
        var current = actor.Recall<LocationResponse>(location);
        var changed = change(current);
        var request = new CreateLocationRequest(changed.Address, changed.Description, changed.TotalCapacity, changed.AverageOccupancy.TrimEnd('%'));

        var api = new LocationsApi(actor.UsingTheApi());
        var response = actor.TheLastResponse();
        var waiter = await actor.AsksForAsync(new TheWaiterSession());

        await response.CaptureAsync(await api.EditLocationAsync(current.Id, request, waiter.IdToken));
        if (!response.Succeeded)
            throw new InvalidOperationException($"Changing the location failed with {response.Status}: {response.Body}");

        var updated = response.Json<LocationResponse>();
        if (location is null)
            actor.Remember(updated);
        else
            actor.Remember(location, updated);
    }
}
