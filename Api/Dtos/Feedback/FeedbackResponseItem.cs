namespace Restaurant_Aqa_Framework.Api.Dtos.Feedback;

public sealed record FeedbackResponseItem(
    string ReservationId,
    string LocationId,
    int Rating,
    DateTime CreatedAt,
    FeedbackType Type,
    string? UserId = null,
    string? Comment = null,
    string? Username = null);
