namespace Restaurant_Aqa_Framework.Api.Dtos.Orders;

public sealed record CreateOrderRequest(string ReservationId, IReadOnlyList<OrderItemRequest> OrderItems);
