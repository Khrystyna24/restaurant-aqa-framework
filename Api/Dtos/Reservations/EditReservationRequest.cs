namespace Restaurant_Aqa_Framework.Api.Dtos.Reservations;

public sealed record EditReservationRequest(
    string ReservationId,
    DateTime StartTime,
    DateTime EndTime,
    int GuestCount);
