using API.Database.Interfaces;
using API.Model.Entities;
using API.Model.Enums;
using API.Model.Shared;
using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace API.Orders.Commands;

public class UpdateOrderToShipping
{
    public record Command(Guid OrderId) : IRequest<Result>;

    public class Handler(IApplicationDbContext context, CurrentUser currentUser) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            if (!currentUser.User!.IsStaff() && !currentUser.User.IsManager())
                return Result.Forbidden();
            int result = await context.Ships
                .Where(x => x.Id == request.OrderId)
                .ExecuteUpdateAsync(x => x.SetProperty(x => x.Status, ShipEnum.Shipping), cancellationToken);
            if (result == 0) return Result.Error("Order not found");
            return Result.NoContent();
        }
    }
}