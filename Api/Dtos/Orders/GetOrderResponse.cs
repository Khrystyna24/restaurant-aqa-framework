namespace Restaurant_Aqa_Framework.Api.Dtos.Orders;

public sealed record GetOrderResponse(
    string OrderId,
    string ReservationId,
    string ReservationAddress,
    string ReservationTime,
    IReadOnlyList<OrderItemResponse> OrderItems,
    OrderStatus Status);
