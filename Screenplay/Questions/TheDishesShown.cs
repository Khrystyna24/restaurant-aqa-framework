using Restaurant_Aqa_Framework.Api.Dtos.Dishes;

namespace Restaurant_Aqa_Framework.Screenplay.Questions;

/// <summary>The dishes the last menu request returned, in the order they were shown.</summary>
public sealed class TheDishesShown : IQuestion<IReadOnlyList<DishListResponse>>
{
    public Task<IReadOnlyList<DishListResponse>> AnsweredByAsync(Actor actor) =>
        Task.FromResult<IReadOnlyList<DishListResponse>>(actor.TheLastResponse().Json<List<DishListResponse>>());
}
