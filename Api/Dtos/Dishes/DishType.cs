namespace Restaurant_Aqa_Framework.Api.Dtos.Dishes;

/// <summary>Request-side only — serializes as the raw member name ("MainCourse"). Response DTOs
/// carry the dish type as a human-readable label string instead (e.g. "Main course"), which these
/// member names do not match, so there is no shared enum between request and response.</summary>
public enum DishType
{
    Appetizer,
    MainCourse,
    Dessert,
}
