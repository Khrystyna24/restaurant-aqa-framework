namespace Restaurant_Aqa_Framework.Api.Dtos.Locations;

public sealed record LocationResponse(
    string Id,
    string Address,
    string Description,
    int TotalCapacity,
    string AverageOccupancy,
    string? ImageUrl,
    double Rating);
