namespace Restaurant_Aqa_Framework.Api.Dtos.Dishes;

/// <summary>Shape returned by GET /dishes/{id} and POST /dishes (single-dish responses) — all fields
/// camelCase, including "fats". Contrast with <see cref="DishListResponse"/>.</summary>
public sealed record DishResponse(
    string Id,
    string Name,
    string Description,
    string DishType,
    decimal Price,
    string Weight,
    string Calories,
    string Proteins,
    string Fats,
    string Carbohydrates,
    string Vitamins,
    string? ImageUrl,
    string State);
