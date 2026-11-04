using Restaurant_Aqa_Framework.Api;
using Restaurant_Aqa_Framework.Api.Dtos.Locations;
using Restaurant_Aqa_Framework.Api.Seeding;

namespace Restaurant_Aqa_Framework.Screenplay.Tasks;

/// <summary>Picks one of the restaurant's own locations to work with. It is remembered like a location the scenario set
/// up itself, but — not being the scenario's own — it is never removed. Locations that other scenarios are creating
/// and removing right now share the same list, so those (and any left-over blank ones) are never chosen.</summary>
public sealed class ChooseAnExistingLocation : ITask
{
    public async Task PerformAsAsync(Actor actor)
    {
        var response = actor.TheLastResponse();
        await response.CaptureAsync(await new LocationsApi(actor.UsingTheApi()).GetLocationsAsync());

        var location = response.Json<List<LocationResponse>>().FirstOrDefault(IsTheRestaurantsOwn)
            ?? throw new InvalidOperationException("The restaurant has no locations of its own to work with.");

        actor.Remember(location);
    }

    private static bool IsTheRestaurantsOwn(LocationResponse location) =>
        !string.IsNullOrWhiteSpace(location.Address)
        && !location.Address.StartsWith(SampleDataFactory.Prefix, StringComparison.Ordinal);
}
