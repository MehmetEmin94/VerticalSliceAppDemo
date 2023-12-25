using ECommerce.API.Entities;

namespace ECommerce.API.Contracts.Orders
{
    public class CreateOrderRequest
    {
        public List<CreateOrderItemRequest> OrderItems { get; set; } = new();
        public string Description { get; set; } = string.Empty;
        public Guid CustomerId { get; set; }
    }

    public class CreateOrderItemRequest
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
}
