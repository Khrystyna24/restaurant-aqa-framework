namespace Restaurant_Aqa_Framework.Api.Dtos.Reservations;

/// <summary><paramref name="CustomerEmail"/>/<paramref name="CustomerId"/> are for waiter-made
/// reservations on behalf of an existing customer only — a self-service customer booking omits both.</summary>
public sealed record CreateReservationRequest(
    string TableId,
    DateTime StartTime,
    DateTime EndTime,
    int GuestCount,
    string? CustomerEmail = null,
    string? CustomerId = null);
