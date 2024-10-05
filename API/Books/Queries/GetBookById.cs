using API.Database.Interfaces;
using API.Model.Entities;
using Ardalis.Result;
using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace API.Books.Queries;

public class GetBookById
{
    public record Query(Guid Id) : IRequest<Result<Response>>;

    public record Response(Guid Id,
        string Title,
        string Description,
        string Author,
        decimal Price);

    public class Handler(IApplicationDbContext context) : IRequestHandler<Query, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            Book? book = await context.Books
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (book is null) return Result.Error("Book not found");

            return Result.Success(new Response(book.Id, book.Title, book.Description, book.Author, book.Price));
        }
    }

    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/books/{id}", async (ISender sender, Guid id) =>
            {
                Result<Response> result = await sender.Send(new Query(id));
                if (!result.IsSuccess) return Results.NotFound(result);
                return Results.Ok(result);
            })
            .WithTags("Books")
            .WithMetadata(new SwaggerOperationAttribute("Get book by id"));
        }
    }
}