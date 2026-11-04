using Restaurant_Aqa_Framework.Api;

namespace Restaurant_Aqa_Framework.Screenplay.Tasks;

/// <summary>Tries to open a dish or a location that is not there ("dish" or "location").</summary>
public sealed class OpenSomethingThatDoesNotExist(string item) : ITask
{
    private const string MissingId = "does-not-exist";

    public async Task PerformAsAsync(Actor actor) =>
        await actor.TheLastResponse().CaptureAsync(item switch
        {
            "dish" => await new DishesApi(actor.UsingTheApi()).GetDishAsync(MissingId),
            "location" => await new LocationsApi(actor.UsingTheApi()).GetLocationAsync(MissingId),
            _ => throw new NotSupportedException($"Only a dish or a location can be opened, not '{item}'."),
        });
}
