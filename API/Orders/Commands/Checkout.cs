using API.Database.Interfaces;
using API.Model.Entities;
using API.Model.Shared;
using Ardalis.Result;
using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace API.Orders.Commands;

public class Checkout
{
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            throw new NotImplementedException();
        }
        public async Task<IResult> Handle(ISender sender)
        {
            Result result = await sender.Send(new Command(), default);
            if (!result.IsSuccess) return TypedResults.BadRequest(result);
            return TypedResults.Created();
        }
    }

    public record Command() : IRequest<Result>;

    public class Handler(IApplicationDbContext context, CurrentUser currentUser) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            if (currentUser.User is null) return Result.Error("User not found");
            Cart? cart = await context.Carts
                .Include(c => c.CartItems).ThenInclude(c => c.Book)
                .Where(c => c.UserId == currentUser.User.Id)
                .FirstOrDefaultAsync(cancellationToken);
            if (cart is null) return Result.Error("Cart not found");
            Guid id = Guid.NewGuid();
            Order creatingOrder = new()
            {
                Id = id,
                UserId = currentUser.User.Id,
                Price = cart.CartItems.Sum(c => c.Quantity * c.Book.Price),
                OrderItems = cart.CartItems.Select(ci => new OrderItem
                {
                    Id = Guid.NewGuid(),
                    BookId = ci.Book.Id,
                    Quantity = ci.Quantity,
                    OrderId = id
                }).ToList()
            };
            context.Orders.Add(creatingOrder);
            context.CartItems.RemoveRange(cart.CartItems);
            context.Carts.Remove(cart);
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
    }
}