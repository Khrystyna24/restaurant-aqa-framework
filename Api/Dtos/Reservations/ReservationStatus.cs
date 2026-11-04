namespace Restaurant_Aqa_Framework.Api.Dtos.Reservations;

/// <summary>No global JsonStringEnumConverter is registered backend-side, so this (like every other
/// status enum in the API) serializes/deserializes as a plain integer on the wire.</summary>
public enum ReservationStatus
{
    Reserved = 0,
    Cancelled = 1,
    InProgress = 2,
    Completed = 3,
}
