using ECommerce.API.Contracts.Orders;

namespace ECommerce.API.Contracts.Customers
{
    public class CustomerResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SurName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<OrderResponse> Orders { get; set; }
    }
}
