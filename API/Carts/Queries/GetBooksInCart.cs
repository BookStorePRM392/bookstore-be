using API.Database.Interfaces;
using API.Model.Entities;
using API.Model.Shared;
using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace API.Carts.Queries;

public class GetBooksInCart
{
    public record Query(int PageIndex = 1, int PageSize = 10) : IRequest<Result<Response>>;

    public record Response(IEnumerable<BooksDetails> Books, int PageIndex, int PageSize, int TotalCount);

    public record BooksDetails(
        Guid Id,
        string Title,
        string Description,
        string Author,
        decimal Price
    );

    public class Handler(IApplicationDbContext context, CurrentUser currentUser) : IRequestHandler<Query, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            IQueryable<CartItem> query = context.CartItems
                .AsNoTracking()
                .Where(x => x.Cart.UserId == currentUser.User!.Id);

            int count = await query.CountAsync(cancellationToken);
            IEnumerable<BooksDetails> books = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x =>
                    new BooksDetails(x.BookId,
                                        x.Book.Title,
                                        x.Book.Description,
                                        x.Book.Author,
                                        x.Book.Price))
                .ToListAsync(cancellationToken);

            return Result.Success(new Response(books,
                                                request.PageIndex,
                                                request.PageSize,
                                                (int)Math.Ceiling((double)count / request.PageSize)));
        }
    }
}