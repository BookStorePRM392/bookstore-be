using API.Database.Interfaces;
using API.Model.Entities;
using API.Model.Enums;
using Ardalis.Result;
using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace API.Users.Commands;

public class SignUp
{
    public record Command(
        string Username,
        string Email,
        string Password,
        string Address,
        string Phone
    ) : IRequest<Result>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            if (await context.Users.AnyAsync(x => x.Email == request.Email, cancellationToken))
                return Result.Error("Email already exists");
            Guid id = Guid.NewGuid();
            User user = new()
            {
                Id = id,
                Username = request.Username,
                Role = UserRoleEnum.Customer,
                Email = request.Email,
                Password = request.Password,
                Address = request.Address,
                Phone = request.Phone,
                Cart = new()
                {
                    Id = Guid.NewGuid(),
                    UserId = id
                }
            };
            context.Users.Add(user);
            await context.SaveChangesAsync(cancellationToken);
            return Result.SuccessWithMessage("User created successfully");
        }
    }

    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/users/signup", async (ISender sender, string Username, string Email, string Password, string Address, string Phone) =>
            {
                Result result = await sender.Send(new Command(Username, Email, Password, Address, Phone));
                if(!result.IsSuccess) return Results.BadRequest(result);
                return Results.Created(result.SuccessMessage, result);
            })
            .WithTags("Users")
            .WithMetadata(new SwaggerOperationAttribute("Create new user"));
        }
    }
}