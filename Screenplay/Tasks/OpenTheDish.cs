using Restaurant_Aqa_Framework.Api;
using Restaurant_Aqa_Framework.Api.Dtos.Dishes;

namespace Restaurant_Aqa_Framework.Screenplay.Tasks;

/// <summary>Opens one dish: the one this scenario added most recently, or the one with the given id.</summary>
public sealed class OpenTheDish(string? id = null) : ITask
{
    public async Task PerformAsAsync(Actor actor) =>
        await actor.TheLastResponse().CaptureAsync(
            await new DishesApi(actor.UsingTheApi()).GetDishAsync(id ?? actor.Recall<DishResponse>().Id));
}
