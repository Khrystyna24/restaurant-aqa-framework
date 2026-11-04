namespace Restaurant_Aqa_Framework.Api.Dtos.Locations;

/// <summary>Used for both POST /locations and PUT /locations/{id} — the backend deserializes
/// PUT's body into this same request type (EditLocationHandler).</summary>
public sealed record CreateLocationRequest(
    string Address,
    string Description,
    int TotalCapacity,
    string AverageOccupancy,
    string? ImageBase64 = null,
    double Rating = 0);
