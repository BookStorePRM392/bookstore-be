using API.Database.Interfaces;
using API.Model.Entities;
using API.Model.Shared;
using Ardalis.Result;
using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace API.Carts.Commands;

public class AddToCart
{
    public record Command(Guid BookId) : IRequest<Result>;

    public class Handler(IApplicationDbContext context, CurrentUser currentUser) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            Book? book = await context.Books
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.BookId, cancellationToken);
            if (book is null) return Result.Error("Book not found");
            CartItem? cartItem = await context.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(x => x.BookId == request.BookId && x.Cart.UserId == currentUser.User!.Id, cancellationToken);
            if (cartItem is not null)
            {
                cartItem.Quantity++;
            }
            else
            {
                Guid cartId = await context.Carts.AsNoTracking().Where(c => c.UserId.Equals(currentUser.User!.Id)).Select(c => c.Id).SingleOrDefaultAsync(cancellationToken);
                cartItem = new CartItem
                {
                    Id = Guid.NewGuid(),
                    CartId = cartId,
                    BookId = book.Id,
                };
                context.CartItems.Add(cartItem);
            }
            await context.SaveChangesAsync(cancellationToken);

            return Result.NoContent();
        }
    }

    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/carts/{bookId}/add", async (ISender sender, Guid bookId) =>
            {
                Result result = await sender.Send(new Command(bookId));
                return Results.Ok(result);
            })
            .WithTags("Carts")
            .WithMetadata(new SwaggerOperationAttribute("Add book to cart"))
            .RequireAuthorization();
        }
    }
}