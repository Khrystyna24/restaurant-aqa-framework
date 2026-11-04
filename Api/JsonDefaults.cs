using System.Text.Json;

namespace Restaurant_Aqa_Framework.Api;

/// <summary>
/// The backend's own default is camelCase (see ResponseBuilder.CreateJsonResponse / most
/// Models' [JsonPropertyName] attributes). Request/response DTOs rely on this policy instead
/// of repeating [JsonPropertyName] on every property; only the rare field that breaks camelCase
/// (e.g. DishListResponse.Fats) needs an explicit override.
/// </summary>
public static class JsonDefaults
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };
}
