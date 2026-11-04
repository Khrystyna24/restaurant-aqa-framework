namespace Restaurant_Aqa_Framework.Api.Dtos.Reservations;

public sealed record ReservationResponse(
    string Id,
    string TableId,
    DateTime StartTime,
    DateTime EndTime,
    int GuestCount,
    ReservationStatus Status,
    string? CustomerId,
    string WaiterId,
    string LocationAddress,
    string RestaurantName,
    bool HasSubmittedPreOrder);
