using Restaurant_Aqa_Framework.Api;
using Restaurant_Aqa_Framework.Api.Dtos.Locations;
using Restaurant_Aqa_Framework.Api.Seeding;
using Restaurant_Aqa_Framework.Screenplay.Questions;

namespace Restaurant_Aqa_Framework.Screenplay.Tasks;

/// <summary>Sets up a restaurant location (a sample one unless given a request), optionally under a name so a
/// scenario can work with several. The outcome is left as the scenario's last response; if the backend accepted
/// it, the location is remembered and scheduled for removal — even when the scenario was testing that it should
/// have been refused.</summary>
public sealed class SetUpLocation(CreateLocationRequest? request = null, string? name = null, bool mustSucceed = true) : ITask
{
    public async Task PerformAsAsync(Actor actor)
    {
        var api = new LocationsApi(actor.UsingTheApi());
        var response = actor.TheLastResponse();

        await response.CaptureAsync(await api.CreateLocationAsync(request ?? SampleDataFactory.CreateLocationRequest()));
        if (!response.Succeeded)
        {
            // A scenario that is testing a refusal passes mustSucceed: false and reads the outcome; any other
            // scenario is only as good as its set-up, so a failed set-up stops it right here.
            if (mustSucceed)
                throw new InvalidOperationException($"Setting up the location failed with {response.Status}: {response.Body}");

            return;
        }

        var location = response.Json<LocationResponse>();
        if (name is null)
            actor.Remember(location);
        else
            actor.Remember(name, location);

        var waiter = await actor.AsksForAsync(new TheWaiterSession());
        actor.Recall<CleanupScope>().TrackDelete(
            $"location:{location.Id}",
            () => api.DeleteLocationAsync(location.Id, force: true, bearerToken: waiter.IdToken));
    }
}
