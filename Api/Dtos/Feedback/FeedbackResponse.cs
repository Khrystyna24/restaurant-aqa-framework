namespace Restaurant_Aqa_Framework.Api.Dtos.Feedback;

public sealed record FeedbackResponse(IReadOnlyList<FeedbackResponseItem> Items, int TotalPages, int CurrentPage);
