using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BilalEmirMergenWebsite.Models;
using Microsoft.IdentityModel.Tokens;

namespace BilalEmirMergenWebsite.Services;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAtUtc) Create(AdminUser user);
}

public sealed class JwtTokenService(IConfiguration configuration) : IJwtTokenService
{
    public (string Token, DateTime ExpiresAtUtc) Create(AdminUser user)
    {
        var key = configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured.");
        var issuer = configuration["Jwt:Issuer"] ?? "BilalEmirMergenWebsite";
        var audience = configuration["Jwt:Audience"] ?? "BilalEmirMergenWebsite.Admin";
        var expires = DateTime.UtcNow.AddHours(2);
        var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        var token = new JwtSecurityToken(issuer, audience, claims, expires: expires, signingCredentials: credentials);
        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
