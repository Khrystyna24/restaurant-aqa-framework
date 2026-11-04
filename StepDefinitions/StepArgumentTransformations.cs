using Restaurant_Aqa_Framework.Api.Dtos.Dishes;
using Restaurant_Aqa_Framework.Support;

namespace Restaurant_Aqa_Framework.StepDefinitions;

/// <summary>
/// Turns the words a step captured into the typed value its step definition wants. Each named
/// transformation is also a custom Cucumber-expression parameter type — a step written with
/// <c>{dish_type}</c> only matches "appetizer", "main course" or "dessert", so a typo in a feature
/// file is an undefined step rather than a runtime parse error. A transformation's regex must
/// contain its own capture group (Reqnroll passes that group's text to the method).
/// </summary>
[Binding]
public static class StepArgumentTransformations
{
    [StepArgumentTransformation(Name = "dish_type", Regex = "(appetizer|main course|dessert)")]
    public static DishType ToDishType(string value) => value switch
    {
        "appetizer" => DishType.Appetizer,
        "main course" => DishType.MainCourse,
        "dessert" => DishType.Dessert,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    [StepArgumentTransformation(Name = "sort_order", Regex = "(price, lowest first|price, highest first|popularity, least popular first|popularity, most popular first)")]
    public static DishSortOrder ToSortOrder(string value) => value switch
    {
        "price, lowest first" => DishSortOrder.PriceAscending,
        "price, highest first" => DishSortOrder.PriceDescending,
        "popularity, least popular first" => DishSortOrder.PopularityAscending,
        "popularity, most popular first" => DishSortOrder.PopularityDescending,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    [StepArgumentTransformation(Name = "price_order", Regex = "(from cheapest to most expensive|from most expensive to cheapest)")]
    public static SortDirection ToPriceOrder(string value) =>
        value == "from cheapest to most expensive" ? SortDirection.Ascending : SortDirection.Descending;

    [StepArgumentTransformation(Name = "field", Regex = "(first name|last name|email address|password confirmation|password)")]
    public static InputField ToField(string value) => value switch
    {
        "first name" => InputField.FirstName,
        "last name" => InputField.LastName,
        "email address" => InputField.EmailAddress,
        "password" => InputField.Password,
        "password confirmation" => InputField.PasswordConfirmation,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    [StepArgumentTransformation(Name = "problem", Regex = "(too short|too long|empty|with digits|with an underscore|without an @ sign|without a domain|different from the password)")]
    public static InputProblem ToProblem(string value) => value switch
    {
        "too short" => InputProblem.TooShort,
        "too long" => InputProblem.TooLong,
        "empty" => InputProblem.Empty,
        "with digits" => InputProblem.WithDigits,
        "with an underscore" => InputProblem.WithAnUnderscore,
        "without an @ sign" => InputProblem.WithoutAtSign,
        "without a domain" => InputProblem.WithoutDomain,
        "different from the password" => InputProblem.DifferentFromPassword,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    [StepArgumentTransformation(Name = "location_problem", Regex = "(an empty address|a negative capacity)")]
    public static LocationProblem ToLocationProblem(string value) => value switch
    {
        "an empty address" => LocationProblem.EmptyAddress,
        "a negative capacity" => LocationProblem.NegativeCapacity,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    [StepArgumentTransformation(Name = "item", Regex = "(dish|location)")]
    public static string ToItem(string value) => value;

    [StepArgumentTransformation(Name = "information", Regex = "(dish catalog|popular dishes|list of locations)")]
    public static string ToInformation(string value) => value;

    [StepArgumentTransformation]
    public static Fields ToFields(DataTable table) => Fields.From(table);
}
