using System.Security.Claims;
using Api.Models.Dtos.Responses;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Api.Security;

public interface ITokenService
{
    string CreateToken(AuthUserInfo user);
}

public class JwtService(IConfiguration config) : ITokenService
{
    public const string SignatureAlgorithm = SecurityAlgorithms.HmacSha512;
    public const string JwtKey = "JwtKey";

    public string CreateToken(AuthUserInfo user)
    {
        var key = Convert.FromBase64String(config.GetValue<string>(JwtKey)!);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SignatureAlgorithm
            ),
            Subject = new ClaimsIdentity(user.ToClaims()),
            Expires = DateTime.UtcNow.AddDays(7),
        };
        var tokenHandler = new JsonWebTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return token;
    }

    public static TokenValidationParameters ValidationParameters(IConfiguration config)
    {
        var key = Convert.FromBase64String(config.GetValue<string>(JwtKey)!);
        return new TokenValidationParameters
        {
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidAlgorithms = [SignatureAlgorithm],
            ValidateIssuerSigningKey = true,
            TokenDecryptionKey = null,

            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,

            // Set to 0 when validating on the same system that created the token
            ClockSkew = TimeSpan.Zero,
        };
    }
}