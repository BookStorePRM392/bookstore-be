using API.Database.Interfaces;
using API.Model.Entities;
using API.Model.Shared;
using Ardalis.Result;
using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace API.Orders.Queries;

public class GetOrderHistory
{
    public record Query(
            int PageIndex,
            int PageSize
        ) : IRequest<Result<Response>>;

    public record Response(
        IEnumerable<OrderDetail> Orders,
        int PageIndex,
        int PageSize,
        int TotalPages
    );

    public record OrderDetail(
        Guid Id,
        Guid UserId,
        string Username,
        decimal Price,
        DateTimeOffset CreatedAt
    );

    public class Hander(IApplicationDbContext context, CurrentUser currentUser) : IRequestHandler<Query, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            IQueryable<Order> query = context.Orders
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .Include(x => x.User)
                .Where(x => x.User.Id.Equals(currentUser.User!.Id));

            int count = await query.CountAsync(cancellationToken);

            IEnumerable<OrderDetail> orders = await query
                .Include(x => x.User)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new OrderDetail(x.Id, x.UserId, x.User.Username, x.Price, x.CreatedAt))
                .ToListAsync(cancellationToken);

            return Result.Success(new Response(orders,
                                            request.PageIndex,
                                            request.PageSize,
                                            (int)Math.Ceiling((double)count / request.PageSize)));
        }
    }

    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("orders/history", async (ISender sender, int pageIndex = 1, int pageSize = 10) =>
            {
                Result<Response> result = await sender.Send(new Query(pageIndex, pageSize));
                return Results.Ok(result);
            }).WithTags("Orders")
            .WithMetadata(new SwaggerOperationAttribute("Get order history"))
            .RequireAuthorization();
        }
    }
}