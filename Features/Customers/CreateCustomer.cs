using Carter;
using ECommerce.API.Contracts.Customers;
using ECommerce.API.Database;
using ECommerce.API.Entities;
using ECommerce.API.Shared;
using FluentValidation;
using Mapster;
using MediatR;

namespace ECommerce.API.Features.Customers
{
    public static class CreateCustomer
    {
        public class Command : IRequest<Result<CustomerResponse>>
        {
            public string Name { get; set; } = string.Empty;
            public string SurName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
        }

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(c => c.Name).NotEmpty().WithMessage("This field cannot be empty.");
                RuleFor(c => c.SurName).NotEmpty().WithMessage("This field cannot be empty.");
                RuleFor(c => c.Email)
                    .NotEmpty().WithMessage("This field cannot be empty.")
                    .EmailAddress().WithMessage("Please enter a valid email address.");
            }
        }

        internal sealed class Handler : IRequestHandler<Command, Result<CustomerResponse>>
        {
            private readonly ApplicationDbContext _context;
            private readonly IValidator<Command> _validator;

            public Handler(ApplicationDbContext context, IValidator<Command> validator)
            {
                _context = context;
                _validator = validator;
            }

            public async Task<Result<CustomerResponse>> Handle(Command request, CancellationToken cancellationToken)
            {
                var validationResult = _validator.Validate(request);
                if (!validationResult.IsValid)
                {
                    return Result<CustomerResponse>.Failure(new Error(
                        "CreateCustomer.Validation",
                        validationResult.ToString()));
                }
                var customer = new Customer
                {
                    Id = Guid.NewGuid(),
                    Name = request.Name,
                    SurName = request.SurName,
                    Email = request.Email,
                };

                _context.Add(customer);
                await _context.SaveChangesAsync();
                return Result<CustomerResponse>.Success(customer.Adapt<CustomerResponse>());
            }
        }
    }

    public class CreateCustomerEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {

            app.MapPost("api/customers", async (CreateCustomerRequest request, ISender sender) =>
            {
                var command = request.Adapt<CreateCustomer.Command>();
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
