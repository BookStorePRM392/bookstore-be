using API.Database.Interfaces;
using API.Model.Entities;
using API.Utils;
using Ardalis.Result;
using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace API.Users.Commands;

public class Login
{
    public record Command(
        string Username,
        string Password) : IRequest<Result<string>>;

    public class Handler(IApplicationDbContext context, TokenProviders tokenProviders) : IRequestHandler<Command, Result<string>>
    {
        public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            User? user = await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Username == request.Username && x.Password == request.Password, cancellationToken);
            if (user is null) return Result.Error("Username or password is incorrect");
            return Result.Success(tokenProviders.GetToken(user));
        }
    }

    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/users/login", async (ISender sender, string Username, string Password) =>
            {
                Result<string> result = await sender.Send(new Command(Username, Password));
                return Results.Ok(result);
            })
            .WithTags("Users")
            .WithMetadata(new SwaggerOperationAttribute("Login user"));
        }
    }
}