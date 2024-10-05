using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using API.Database.Interfaces;
using API.Model.Entities;
using API.Model.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace API.Middlewares;

public class AuthMiddleware(IApplicationDbContext dbContext, IConfiguration configuration) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // configure issuer
        string issuer = configuration.GetValue<string>("JWT:Issuer") ?? throw new ArgumentNullException("JWT:Issuer");
        string? authHeader = context.Request.Headers.Authorization.ToString();
        if (authHeader.IsNullOrEmpty() || !authHeader.Contains("Bearer "))
        {
            await next.Invoke(context);
            return;
        }
        // get token
        authHeader = authHeader.Replace("Bearer ", "");
        JwtSecurityTokenHandler? handler = new();
        JwtSecurityToken? jwtSecurityToken = handler.ReadJwtToken(authHeader);
        IEnumerable<Claim>? claims = jwtSecurityToken.Claims;
        if (!claims.Any(c => c.Issuer.Equals(issuer, StringComparison.InvariantCultureIgnoreCase)))
        {
            await next.Invoke(context);
            return;
        }
        // check user
        if (!Guid.TryParse(claims.FirstOrDefault(c =>
                    c.Type.Equals("sub", StringComparison.InvariantCultureIgnoreCase)
                )
                ?.Value ?? string.Empty, out Guid id)) throw new ArgumentException();

        User? checkingUser = await dbContext
            .Users
            .AsNoTracking().SingleOrDefaultAsync(u => u.Id.Equals(id));
        if (checkingUser is null)
        {
            await next.Invoke(context);
            return;
        }
        CurrentUser currentUser = context.RequestServices.GetRequiredService<CurrentUser>();
        currentUser.User = checkingUser;
        await next.Invoke(context);
    }
}