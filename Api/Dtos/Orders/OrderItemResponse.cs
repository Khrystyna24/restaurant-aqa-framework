using Restaurant_Aqa_Framework.Api.Dtos.Dishes;

namespace Restaurant_Aqa_Framework.Api.Dtos.Orders;

public sealed record OrderItemResponse(DishResponse Dish, int Quantity);
