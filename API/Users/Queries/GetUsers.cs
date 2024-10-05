using API.Database.Interfaces;
using API.Model.Entities;
using Ardalis.Result;
using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace API.Users.Queries;

public class GetUsers
{
    public record Query(int PageIndex = 1,
     int PageSize = 10) : IRequest<Result<Response>>;

    public record Response(
        IEnumerable<UserDetails> Users,
        int PageIndex,
        int PageSize,
        int TotalPages
    );

    public record UserDetails(Guid Id, string Username, string Email, string Address, string Phone);

    public class Handler(IApplicationDbContext context) : IRequestHandler<Query, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            IQueryable<User> query = context.Users
                .AsNoTracking()
                .OrderBy(x => x.Username);
            int count = await query.CountAsync(cancellationToken);

            IEnumerable<UserDetails> users = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new UserDetails(x.Id,
                x.Username,
                x.Email,
                x.Address,
                x.Phone))
                .ToListAsync(cancellationToken);

            int totalPages = (int)Math.Ceiling((double)count / request.PageSize);

            return Result.Success(new Response(users, request.PageIndex, request.PageSize, totalPages));
        }
    }

    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("users", async (ISender sender, int pageIndex = 1, int pageSize = 10) =>
            {
                Result<Response> result = await sender.Send(new Query(pageIndex, pageSize));
                return Results.Ok(result);
            }).WithTags("Users")
            .WithMetadata(new SwaggerOperationAttribute("Get users"));
        }
    }
}