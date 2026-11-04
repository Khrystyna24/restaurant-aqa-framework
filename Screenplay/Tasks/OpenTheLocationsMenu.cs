using Restaurant_Aqa_Framework.Api;
using Restaurant_Aqa_Framework.Api.Dtos.Locations;

namespace Restaurant_Aqa_Framework.Screenplay.Tasks;

/// <summary>Opens the menu of one location (the named one, or else the most recent).</summary>
public sealed class OpenTheLocationsMenu(string? location = null) : ITask
{
    public async Task PerformAsAsync(Actor actor) =>
        await actor.TheLastResponse().CaptureAsync(
            await new LocationsApi(actor.UsingTheApi()).GetDishesForLocationAsync(actor.Recall<LocationResponse>(location).Id));
}
