using API.Database.Interfaces;
using API.Model.Entities;
using Ardalis.Result;
using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace API.Books.Commands;

public class DeleteBook
{
    public record Command(Guid Id) : IRequest<Result>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            Book? book = await context.Books
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (book == null) return Result.Error("Book not found");
            await context.OrderItems
                .Where(x => x.BookId == request.Id)
                .ExecuteDeleteAsync(cancellationToken);
            await context.CartItems
                .Where(x => x.BookId == request.Id)
                .ExecuteDeleteAsync(cancellationToken);
            await context.Books.Where(x => x.Id == request.Id)
                .ExecuteDeleteAsync(cancellationToken);
            return Result.NoContent();
        }
    }

    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("/books/{id}", async (ISender sender, Guid id) =>
            {
                Result result = await sender.Send(new Command(id));
                return Results.NoContent();
            })
            .WithTags("Books")
            .WithMetadata(new SwaggerOperationAttribute("Delete book by id"))
            .RequireAuthorization();
        }
    }
}