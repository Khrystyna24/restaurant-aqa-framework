namespace Restaurant_Aqa_Framework.Api.Dtos.Orders;

public sealed record OrderItemRequest(string DishId, int Quantity, OrderItemState State = OrderItemState.Pending);
