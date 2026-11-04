namespace Restaurant_Aqa_Framework.Api.Dtos.Feedback;

public sealed record CreateFeedbackRequest(
    string ReservationId,
    int Rating,
    FeedbackType Type,
    string? UserId = null,
    string? Comment = null);
