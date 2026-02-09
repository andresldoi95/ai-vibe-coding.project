using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SaaS.Application.Common.Interfaces;
using SaaS.Domain.Entities;
using BCrypt.Net;

namespace SaaS.Infrastructure.Services;

/// <summary>
/// Authentication service handling password hashing and JWT token generation
/// </summary>
public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly IUserRepository _userRepository;

    public AuthService(IConfiguration configuration, IUserRepository userRepository)
    {
        _configuration = configuration;
        _userRepository = userRepository;
    }

    public async Task<User?> ValidateCredentialsAsync(string email, string password)
    {
        var user = await _userRepository.GetByEmailAsync(email);

        if (user == null || !user.IsActive)
            return null;

        if (!VerifyPassword(password, user.PasswordHash))
            return null;

        return user;
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }

    public string GenerateJwtToken(User user, List<Guid> tenantIds)
    {
        var jwtSettings = _configuration.GetSection("JWT");
        var secret = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
        var issuer = jwtSettings["Issuer"] ?? "SaaS.Backend";
        var audience = jwtSettings["Audience"] ?? "SaaS.Frontend";
        var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "15");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Name, user.Name),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        // Add tenant IDs as separate claims
        foreach (var tenantId in tenantIds)
        {
            claims.Add(new Claim("tenants", tenantId.ToString()));
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateJwtTokenWithRole(User user, Guid tenantId, Role role, List<string> permissions)
    {
        var jwtSettings = _configuration.GetSection("JWT");
        var secret = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
        var issuer = jwtSettings["Issuer"] ?? "SaaS.Backend";
        var audience = jwtSettings["Audience"] ?? "SaaS.Frontend";
        var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "15");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Name, user.Name),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("tenant_id", tenantId.ToString()),
            new Claim("role_id", role.Id.ToString()),
            new Claim("role_name", role.Name),
        };

        // Add permissions as individual claims for easy policy-based authorization
        foreach (var permission in permissions)
        {
            claims.Add(new Claim("permission", permission));
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public RefreshToken GenerateRefreshToken(string ipAddress)
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        return new RefreshToken
        {
            Token = Convert.ToBase64String(randomBytes),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = ipAddress
        };
    }
}
