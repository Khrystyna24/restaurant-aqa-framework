using System.Text.Json.Serialization;

namespace Restaurant_Aqa_Framework.Api.Dtos.Dishes;

public sealed record CreateDishRequest(
    string Name,
    string Description,
    [property: JsonConverter(typeof(JsonStringEnumConverter))] DishType DishType,
    decimal Price,
    string Weight,
    string Calories,
    string Proteins,
    string Fats,
    string Carbohydrates,
    string Vitamins,
    IReadOnlyList<string> LocationIds,
    [property: JsonConverter(typeof(JsonStringEnumConverter))] DishState State,
    string? ImageBase64 = null);
