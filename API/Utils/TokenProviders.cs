using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Model.Entities;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace API.Utils;

public class TokenProviders(IConfiguration configuration)
{
    public string GetToken(User user)
    {
        string secretKey = configuration.GetValue<string>("JWT:SecretKey") ?? throw new ArgumentNullException("JWT:SecretKey");
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString())
            ]),
            Expires = DateTime.UtcNow.AddMinutes(30),
            Issuer = configuration.GetValue<string>("JWT:Issuer"),
            Audience = configuration.GetValue<string>("JWT:Audience"),
            SigningCredentials = credentials,
        };
        var handler = new JsonWebTokenHandler();
        var tokenString = handler.CreateToken(token);
        return tokenString;
    }
}