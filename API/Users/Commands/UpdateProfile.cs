using API.Database.Interfaces;
using API.Model.Shared;
using Ardalis.Result;
using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace API.Users.Commands;

public class UpdateProfile
{
    public record UpdateProfileRequest(
        string username,
        string email,
        string address,
        string phone
    );
    public record Command(
        string Username,
        string Email,
        string Address,
        string Phone
    ) : IRequest<Result>;

    public class Handler(IApplicationDbContext context, CurrentUser currentUser) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            int result = await context.Users
                .Where(x => x.Id == currentUser.User!.Id)
                .ExecuteUpdateAsync(x => x
                    .SetProperty(x => x.Username, request.Username)
                    .SetProperty(x => x.Email, request.Email)
                    .SetProperty(x => x.Address, request.Address)
                    .SetProperty(x => x.Phone, request.Phone), cancellationToken
                );

            if (result == 0) return Result.Error("Profile not found");
            return Result.Success();
        }
    }

    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("users/profile", async (ISender sender, UpdateProfileRequest request) =>
            {
                Result result = await sender.Send(new Command(
                    request.username,
                    request.email,
                    request.address,
                    request.phone
                ));
                return Results.NoContent();
            })
            .WithTags("Users")
            .WithMetadata(new SwaggerOperationAttribute("Update profile"))
            .RequireAuthorization();
        }
    }
}