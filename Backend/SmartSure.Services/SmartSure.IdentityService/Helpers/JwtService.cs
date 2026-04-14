using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace SmartSure.IdentityService.Helpers;

/// <summary>
/// Builds signed JWT access tokens and generates cryptographically random refresh tokens.
/// </summary>
public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Constructs a signed JWT containing identity claims (sub, email, roles) and
    /// multiple audience claims. The token is signed with HMAC-SHA256 and expires in 60 minutes.
    /// </summary>
    public string BuildToken(string key, string issuer, IEnumerable<string> audiences, string userId, string email, IEnumerable<string> roles)
    {
        // Build the standard identity claims
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, email),
            new(ClaimTypes.Email, email),
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.UniqueName, email)
        };

        // Add each configured audience as a separate claim
        if (audiences != null)
        {
            foreach (var audience in audiences)
            {
                claims.Add(new Claim(JwtRegisteredClaimNames.Aud, audience));
            }
        }

        // Add role claims so [Authorize(Roles = "...")] works out of the box
        if (roles != null)
        {
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        // audience is set to null here because audiences are embedded as individual claims above
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: null,
            claims: claims,
            expires: DateTime.Now.Add(TimeSpan.FromMinutes(60)),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Generates a 32-byte cryptographically random refresh token encoded as Base64.
    /// This token is stored on the user record and rotated on every use.
    /// </summary>
    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}