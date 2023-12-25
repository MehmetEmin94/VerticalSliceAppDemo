namespace ECommerce.API.Contracts.Orders
{
    public class OrderResponse
    {
        public Guid Id { get; set; }
        public List<OrderItemResponse> OrderItems { get; set; } = new();
        public string Description { get; set; } = string.Empty;
        public decimal OrderPrice => OrderItems.Select(orderItem => orderItem.Price).Sum();
        public Guid CustomerId { get; set; }
    }

    public class OrderItemResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
}
