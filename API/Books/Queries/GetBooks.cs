using API.Database.Interfaces;
using API.Model.Entities;
using Ardalis.Result;
using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace API.Books.Queries;

public class GetBooks
{
    public record Query(
        int PageIndex,
        int PageSize,
        string Keyword
    ) : IRequest<Result<Response>>;

    public record Response(
        IEnumerable<BooksDetails> Books,
        int PageIndex,
        int PageSize,
        int TotalPages
    );

    public record BooksDetails(
        Guid Id,
        string Title,
        string Description,
        string Author,
        decimal Price
    );

    public class Handler(IApplicationDbContext context) : IRequestHandler<Query, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            IQueryable<Book> query = context.Books
                                            .AsNoTracking()
                                            .Where(b => b.Title.Trim().ToLower().Contains(request.Keyword.Trim().ToLower()));

            int count = await query.CountAsync(cancellationToken);

            IEnumerable<BooksDetails> books = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(b => new BooksDetails(b.Id, b.Title, b.Description, b.Author, b.Price))
                .ToListAsync(cancellationToken);

            return Result.Success(new Response(books, request.PageIndex, request.PageSize, (int)Math.Ceiling((double)count / request.PageSize)));
        }
    }

    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/books", async (ISender sender, int pageIndex = 1, int pageSize = 10, string keyword = "") =>
            {
                Result<Response> result = await sender.Send(new Query(pageIndex, pageSize, keyword));
                return Results.Ok(result);
            })
            .WithTags("Books")
            .WithMetadata(new SwaggerOperationAttribute("Get books by keyword"));
        }
    }
}