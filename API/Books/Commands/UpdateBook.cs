using API.Database.Interfaces;
using Ardalis.Result;
using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace API.Books.Commands;

public class UpdateBook
{
    public record Command(Guid Id,
        string Title,
        string Description,
        string Author,
        decimal Price) : IRequest<Result>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            int result = await context
                .Books
                .Where(x => x.Id == request.Id)
                .ExecuteUpdateAsync(x => x
                    .SetProperty(x => x.Title, request.Title)
                    .SetProperty(x => x.Description, request.Description)
                    .SetProperty(x => x.Author, request.Author)
                    .SetProperty(x => x.Price, request.Price), cancellationToken
                );

            if (result == 0) return Result.Error("Book not found");
            return Result.NoContent();
        }
    }

    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/books/{id}", async (ISender sender, Guid id, string Title, string Description, string Author, decimal Price) =>
            {
                Result result = await sender.Send(new Command(id, Title, Description, Author, Price));
                return Results.NoContent();
            })
            .WithTags("Books")
            .WithMetadata(new SwaggerOperationAttribute("Update book by id"));
        }
    }
}