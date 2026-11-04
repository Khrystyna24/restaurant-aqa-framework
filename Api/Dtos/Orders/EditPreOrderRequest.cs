namespace Restaurant_Aqa_Framework.Api.Dtos.Orders;

public sealed record EditPreOrderRequest(IReadOnlyList<OrderItemRequest> OrderItems);
