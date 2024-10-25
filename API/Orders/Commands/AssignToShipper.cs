using API.Database.Interfaces;
using API.Model.Entities;
using API.Model.Shared;
using Ardalis.Result;
using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace API.Orders.Commands;

public class AssignToShipper
{
    public record Command(
        Guid OrderId,
        Guid UserId
    ) : IRequest<Result>;

    public class Handler(IApplicationDbContext context, CurrentUser currentUser) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            if (!(currentUser.User!.IsStaff() && currentUser.User.IsManager())) return Result.Forbidden();
            Order? order = await context.Orders
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.OrderId, cancellationToken);
            if (order is null) return Result.Error("Order not found");
            Ship? ship = await context.Ships.AsNoTracking()
                                            .Where(s => s.OrderId.Equals(request.OrderId))
                                            .FirstOrDefaultAsync(cancellationToken);
            if (ship is not null) return Result.Error("Order already assigned to a shipper");
            User? user = await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.UserId, cancellationToken);
            if (user is null) return Result.Error("User not found");
            ship = new Ship
            {
                Id = Guid.NewGuid(),
                OrderId = request.OrderId,
                ShipperId = request.UserId
            };
            context.Ships.Add(ship);
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }

    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("orders/{orderId}/assign", async (ISender sender, Guid orderId, Guid userId) =>
            {
                Result result = await sender.Send(new Command(orderId, userId));
                return Results.Ok(result);
            })
            .WithTags("Orders")
            .WithMetadata(new SwaggerOperationAttribute("Assign order to shipper"))
            .RequireAuthorization();
        }
    }
}