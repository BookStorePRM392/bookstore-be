using API.Database.Interfaces;
using API.Model.Entities;
using API.Model.Shared;
using Ardalis.Result;
using Carter;
using MediatR;
using Swashbuckle.AspNetCore.Annotations;

namespace API.Books.Commands;

public class CreateBook
{
    public record Command(
        string Title,
        string Description,
        string Author,
        decimal Price
    ) : IRequest<Result<Response>>;

    public record Response(Guid Id);

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Command request, CancellationToken cancellationToken)
        {
            Book addingBook = new()
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Description = request.Description,
                Author = request.Author,
                Price = request.Price
            };
            context.Books.Add(addingBook);
            await context.SaveChangesAsync(cancellationToken);
            return Result.Success(new Response(addingBook.Id), "Book added successfully");
        }
    }

    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/books", async (ISender sender, Command command) =>
            {
                Result<Response> response = await sender.Send(command);
                if (!response.IsSuccess) return Results.BadRequest(response);
                return Results.Created(response.SuccessMessage, response);
            })
            .WithTags("Books")
            .WithMetadata(new SwaggerOperationAttribute("Create new book"))
            .RequireAuthorization();
        }
    }
}