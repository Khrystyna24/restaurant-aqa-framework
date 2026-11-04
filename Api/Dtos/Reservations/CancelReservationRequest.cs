using System.Text.Json.Serialization;

namespace Restaurant_Aqa_Framework.Api.Dtos.Reservations;

/// <summary>Confirmed against backend source (CancelReservationRequest.cs): the wire key is "id",
/// not "reservationId" — the one request DTO in this API that breaks the camelCase-of-property-name
/// convention, so it needs an explicit override instead of relying on <see cref="JsonDefaults"/>.</summary>
public sealed record CancelReservationRequest([property: JsonPropertyName("id")] string ReservationId);
