using Restaurant_Aqa_Framework.Api;
using Restaurant_Aqa_Framework.Api.Dtos.Dishes;

namespace Restaurant_Aqa_Framework.Screenplay.Tasks;

/// <summary>Looks at the menu, optionally narrowed to a category, sorted, or searched by name. The dishes shown are the
/// scenario's last response — read them with <c>TheDishesShown</c>.</summary>
public sealed class BrowseTheMenu(DishType? category = null, DishSortOrder? sortedBy = null, string? nameContains = null) : ITask
{
    public async Task PerformAsAsync(Actor actor) =>
        await actor.TheLastResponse().CaptureAsync(
            await new DishesApi(actor.UsingTheApi()).GetDishesAsync(category?.ToQueryValue(), sortedBy, nameContains));
}
