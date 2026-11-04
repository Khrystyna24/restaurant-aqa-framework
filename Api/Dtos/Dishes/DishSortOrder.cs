namespace Restaurant_Aqa_Framework.Api.Dtos.Dishes;

/// <summary>GET /dishes?sortBy=... accepts these exact strings (APIHandler.Constants.Dishes.DishSortingLabels)
/// — not the enum's own member names serialized any other way, so this is deliberately a lookup, not
/// a [JsonConverter]-driven enum.</summary>
public enum DishSortOrder
{
    PriceAscending,
    PriceDescending,
    PopularityAscending,
    PopularityDescending,
}

public static class DishSortOrderExtensions
{
    public static string ToQueryValue(this DishSortOrder value) => value switch
    {
        DishSortOrder.PriceAscending => "PriceAscending",
        DishSortOrder.PriceDescending => "PriceDescending",
        DishSortOrder.PopularityAscending => "PopularityAscending",
        DishSortOrder.PopularityDescending => "PopularityDescending",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };
}
