using Restaurant_Aqa_Framework.Api;

namespace Restaurant_Aqa_Framework.Screenplay.Tasks;

/// <summary>Asks for one of the restaurant's public lists: "dish catalog", "popular dishes" or "list of locations".</summary>
public sealed class AskForInformation(string information) : ITask
{
    public async Task PerformAsAsync(Actor actor) =>
        await actor.TheLastResponse().CaptureAsync(information switch
        {
            "dish catalog" => await new DishesApi(actor.UsingTheApi()).GetDishesAsync(),
            "popular dishes" => await new DishesApi(actor.UsingTheApi()).GetPopularDishesAsync(),
            "list of locations" => await new LocationsApi(actor.UsingTheApi()).GetLocationsAsync(),
            _ => throw new NotSupportedException($"There is no public list called '{information}'."),
        });
}
