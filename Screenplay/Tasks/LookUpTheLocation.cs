using Restaurant_Aqa_Framework.Api;
using Restaurant_Aqa_Framework.Api.Dtos.Locations;

namespace Restaurant_Aqa_Framework.Screenplay.Tasks;

/// <summary>Looks up a location (the named one, or else the most recent) as a guest would.</summary>
public sealed class LookUpTheLocation(string? location = null) : ITask
{
    public async Task PerformAsAsync(Actor actor) =>
        await actor.TheLastResponse().CaptureAsync(
            await new LocationsApi(actor.UsingTheApi()).GetLocationAsync(actor.Recall<LocationResponse>(location).Id));
}
