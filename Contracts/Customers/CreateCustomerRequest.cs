namespace ECommerce.API.Contracts.Customers
{
    public class CreateCustomerRequest
    {
        public string Name { get; set; } = string.Empty;
        public string SurName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
