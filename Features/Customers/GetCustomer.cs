using Carter;
using ECommerce.API.Contracts.Customers;
using ECommerce.API.Contracts.Orders;
using ECommerce.API.Database;
using ECommerce.API.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Features.Customers
{
    public static class GetCustomer
    {
        public class Query : IRequest<Result<CustomerResponse>>
        {
            public Guid Id { get; set; }
        }

        internal sealed class Handler : IRequestHandler<Query, Result<CustomerResponse>>
        {
            private readonly ApplicationDbContext _context;

            public Handler(ApplicationDbContext context)
            {
                _context = context;
            }

            public async Task<Result<CustomerResponse>> Handle(Query request, CancellationToken cancellationToken)
            {
                var customerResponse = await _context
                    .Customers
                    .Include(customer => customer.Orders)
                    .ThenInclude(order => order.OrderItems)
                    .Where(customer => customer.Id == request.Id)
                    .Select(customer => new CustomerResponse
                    {
                        Id = customer.Id,
                        Name = customer.Name,
                        SurName = customer.SurName,
                        Email = customer.Email,
                        Orders = customer.Orders.Select(order => new OrderResponse
                        {
                            Id = order.Id,
                            OrderItems = order.OrderItems.Select(orderItem => new OrderItemResponse
                            {
                                Id = orderItem.Id,
                                Name = orderItem.Name,
                                Price = orderItem.Price
                            }).ToList(),
                            Description = order.Description,
                            CustomerId = order.CustomerId
                        }).ToList()
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (customerResponse is null)
                {
                    return Result<CustomerResponse>.Failure(new Error(
                        "GetCustomer.Null",
                        "Customer was not found"));
                }

                return Result<CustomerResponse>.Success(customerResponse);
            }
        }
    }

    public class GetCustomerEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("api/customers/{id}", async (Guid id, ISender sender) =>
            {
                var query = new GetCustomer.Query { Id = id };

                var result = await sender.Send(query);

                if (result.IsFailure)
                {
                    return Results.NotFound(result.Error);
                }
                return Results.Ok(result.Value);
            });
        }
    }
}
