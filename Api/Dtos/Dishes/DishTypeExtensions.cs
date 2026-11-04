namespace Restaurant_Aqa_Framework.Api.Dtos.Dishes;

public static class DishTypeExtensions
{
    /// <summary>The value GET /dishes?dishType=... parses (case-insensitive enum member name).</summary>
    public static string ToQueryValue(this DishType value) => value.ToString();

    /// <summary>The human-readable label the backend returns in a dish's <c>dishType</c> field.</summary>
    public static string ToLabel(this DishType value) => value switch
    {
        DishType.Appetizer => "Appetizer",
        DishType.MainCourse => "Main course",
        DishType.Dessert => "Dessert",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };
}
