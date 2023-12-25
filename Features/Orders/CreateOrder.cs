using Carter;
using ECommerce.API.Contracts.Orders;
using ECommerce.API.Database;
using ECommerce.API.Entities;
using ECommerce.API.Shared;
using FluentValidation;
using Mapster;
using MediatR;

namespace ECommerce.API.Features.Orders
{

    public static class CreateOrder
    {
        public class Command : IRequest<Result<OrderResponse>>
        {
            public List<OrderItem> OrderItems { get; set; } = new();
            public string Description { get; set; } = string.Empty;
            public Guid CustomerId { get; set; }
        }

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(c => c.CustomerId).NotEmpty().WithMessage("This field cannot be empty.");
                RuleFor(c => c.OrderItems).NotEmpty().WithMessage("This field cannot be empty.");
                RuleForEach(p => p.OrderItems).ChildRules(child =>
                {
                    child.RuleFor(x => x.Name).NotEmpty();
                    child.RuleFor(x => x.Price).GreaterThan(0);
                });
            }
        }

        internal sealed class Handler : IRequestHandler<Command, Result<OrderResponse>>
        {
            private readonly ApplicationDbContext _context;
            private readonly IValidator<Command> _validator;

            public Handler(ApplicationDbContext context, IValidator<Command> validator)
            {
                _context = context;
                _validator = validator;
            }

            public async Task<Result<OrderResponse>> Handle(Command request, CancellationToken cancellationToken)
            {
                var validationResult = _validator.Validate(request);

                if (!validationResult.IsValid)
                {
                    return Result<OrderResponse>.Failure(new Error(
                        "CreateOrder.Validation",
                        validationResult.ToString()));
                }

                var order = new Order
                {
                    OrderItems = request.OrderItems,
                    Description = request.Description,
                    CustomerId = request.CustomerId,
                };

                _context.Add(order);
                await _context.SaveChangesAsync();
                return Result<OrderResponse>.Success(order.Adapt<OrderResponse>());
            }
        }
    }

    public class CreateOrderEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {

            app.MapPost("api/orders", async (CreateOrderRequest request, ISender sender) =>
            {
                var command = request.Adapt<CreateOrder.Command>();
                var result = await sender.Send(command);

                if (result.IsFailure)
                {
                    return Results.BadRequest(result.Error);
                }

                return Results.Ok(result.Value);
            });

        }
    }
}
