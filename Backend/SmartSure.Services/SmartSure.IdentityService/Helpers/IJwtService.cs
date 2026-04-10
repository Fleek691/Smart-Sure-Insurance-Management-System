namespace SmartSure.IdentityService.Helpers;

public interface IJwtService
{
    string BuildToken(string key, string issuer, IEnumerable<string> audiences, string userId, string email, IEnumerable<string> roles);
    string GenerateRefreshToken();
}