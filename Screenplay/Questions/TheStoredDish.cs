using Restaurant_Aqa_Framework.Api;
using Restaurant_Aqa_Framework.Api.Dtos.Dishes;

namespace Restaurant_Aqa_Framework.Screenplay.Questions;

/// <summary>The dish this scenario added most recently, as the restaurant now holds it (not as it was when added).</summary>
public sealed class TheStoredDish : IQuestion<DishResponse>
{
    public async Task<DishResponse> AnsweredByAsync(Actor actor) =>
        await (await new DishesApi(actor.UsingTheApi()).GetDishAsync(actor.Recall<DishResponse>().Id)).ReadAsAsync<DishResponse>();
}
