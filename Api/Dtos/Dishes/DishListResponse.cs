using System.Text.Json.Serialization;

namespace Restaurant_Aqa_Framework.Api.Dtos.Dishes;

/// <summary>Shape returned by GET /dishes, GET /dishes/popular and GET /locations/{id}/dishes.
/// Confirmed-live backend casing bug: every field is camelCase except "Fats", which is PascalCase
/// (DishListResponse.cs carries a [DynamoDBProperty] attribute on Fats instead of [JsonPropertyName],
/// so it falls back to the raw C# property name). GET /dishes/{id} does not have this bug — see
/// <see cref="DishResponse"/>.</summary>
public sealed record DishListResponse(
    string Id,
    string Name,
    string? ImageUrl,
    decimal Price,
    string State,
    string Weight,
    string DishType,
    string Description,
    string Calories,
    string Proteins,
    [property: JsonPropertyName("Fats")] string Fats,
    string Carbohydrates,
    string Vitamins);
