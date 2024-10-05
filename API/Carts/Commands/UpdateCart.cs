using API.Database.Interfaces;
using API.Model.Shared;
using Ardalis.Result;
using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace API.Carts.Commands;

public class UpdateCart
{
    public record Command(Guid BookId, int Quantity) : IRequest<Result>;

    public class Handler(IApplicationDbContext context, CurrentUser currentUser) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            int result = await context.CartItems
                .Include(ci => ci.Cart)
                .Where(x => x.BookId == request.BookId && x.Cart.UserId == currentUser.User!.Id)
                .ExecuteUpdateAsync(e => e.SetProperty(x => x.Quantity, request.Quantity), cancellationToken);
            if (result == 0) return Result.Error("Book not found");
            return Result.NoContent();
        }
    }

    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/carts/{BookId:int}/{Quantity:int}", async (ISender sender, Guid BookId, int Quantity) =>
            {
                Result result = await sender.Send(new Command(BookId, Quantity));
                if (!result.IsSuccess) return Results.BadRequest(result);
                return Results.NoContent();
            }).WithTags("Carts").WithMetadata(new SwaggerOperationAttribute("Update cart quantity"));
        }
    }
}