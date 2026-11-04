namespace Restaurant_Aqa_Framework.Api.Dtos.Dishes;

/// <summary>Request-side only — serializes as the raw member name ("OnStop"). See <see cref="DishType"/>
/// for why response DTOs use a plain string instead of this enum.</summary>
public enum DishState
{
    OnStop,
    Available,
    OutOfStock,
}
