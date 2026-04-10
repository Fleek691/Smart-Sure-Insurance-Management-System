using Google.Apis.Auth;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using SmartSure.IdentityService.DTOs;
using SmartSure.IdentityService.Helpers;
using SmartSure.IdentityService.Models;
using SmartSure.IdentityService.Repositories;
using SmartSure.Shared.Constants;
using SmartSure.Shared.Events;
using SmartSure.Shared.Exceptions;

namespace SmartSure.IdentityService.Services;

public class GoogleAuthService : IGoogleAuthService
{
    private readonly IConfiguration _configuration;
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IJwtService _jwtService;
    private readonly IUserRegisteredEventPublisher _eventPublisher;
    private readonly IMemoryCache _memoryCache;
    private readonly HttpClient _httpClient;

    public GoogleAuthService(
        IConfiguration configuration,
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IJwtService jwtService,
        IUserRegisteredEventPublisher eventPublisher,
        IMemoryCache memoryCache,
        HttpClient httpClient)
    {
        _configuration = configuration;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _jwtService = jwtService;
        _eventPublisher = eventPublisher;
        _memoryCache = memoryCache;
        _httpClient = httpClient;
    }

    public string GetGoogleLoginUrl()
    {
        var clientId = _configuration["Google:ClientId"] ?? throw new InvalidOperationException("Google:ClientId is missing.");
        var redirectUri = _configuration["Google:RedirectUri"] ?? throw new InvalidOperationException("Google:RedirectUri is missing.");

        var query = new Dictionary<string, string?>
        {
            ["client_id"] = clientId,
            ["redirect_uri"] = redirectUri,
            ["response_type"] = "code",
            ["scope"] = "openid email profile",
            ["access_type"] = "offline",
            ["prompt"] = "select_account"
        };

        return QueryHelpers.AddQueryString("https://accounts.google.com/o/oauth2/v2/auth", query!);
    }

    public async Task<AuthResponseDto> ProcessGoogleCallbackAsync(GoogleCallbackDto dto)
    {
        var code = dto.Code;
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ValidationException("Google authorization code is required.");
        }

        var tokenResponse = await ExchangeGoogleAuthorizationCodeAsync(code);
        if (string.IsNullOrWhiteSpace(tokenResponse.IdToken))
        {
            throw new UnauthorizedException("Google did not return an id token.");
        }

        var payload = await ValidateGoogleTokenAsync(tokenResponse.IdToken);
        var userInfo = MapGoogleUserInfo(payload);
        return await SignInGoogleUserAsync(userInfo);
    }

    private async Task<GoogleJsonWebSignature.Payload> ValidateGoogleTokenAsync(string idToken)
    {
        var clientId = _configuration["Google:ClientId"] ?? throw new InvalidOperationException("Google:ClientId is missing.");
        var validationSettings = new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = new[] { clientId }
        };

        var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, validationSettings);

        if (payload.EmailVerified is not true)
        {
            throw new UnauthorizedException("Google email is not verified.");
        }

        if (string.IsNullOrWhiteSpace(payload.Email) || string.IsNullOrWhiteSpace(payload.Subject))
        {
            throw new UnauthorizedException("Google token is missing required claims.");
        }

        return payload;
    }

    private async Task<AuthResponseDto> SignInGoogleUserAsync(GoogleUserInfoDto userInfo)
    {
        var user = await _userRepository.GetByEmailAsync(userInfo.Email);
        var createdUser = false;

        if (user is null)
        {
            var customerRole = await _roleRepository.GetByNameAsync(Roles.Customer)
                               ?? new Role { RoleId = Guid.NewGuid(), RoleName = Roles.Customer };

            user = new User
            {
                UserId = Guid.NewGuid(),
                FullName = userInfo.Name,
                Email = userInfo.Email,
                GoogleSubject = userInfo.Sub,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UserRoles = new List<UserRole>()
            };

            user.UserRoles.Add(new UserRole { Role = customerRole, RoleId = customerRole.RoleId });
            await _userRepository.AddAsync(user);
            createdUser = true;
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(user.GoogleSubject) && !string.Equals(user.GoogleSubject, userInfo.Sub, StringComparison.Ordinal))
            {
                throw new UnauthorizedException("Google account does not match this user.");
            }

            if (string.IsNullOrWhiteSpace(user.GoogleSubject))
            {
                user.GoogleSubject = userInfo.Sub;
            }

            if (string.IsNullOrWhiteSpace(user.FullName) && !string.IsNullOrWhiteSpace(userInfo.Name))
            {
                user.FullName = userInfo.Name;
            }
        }

        var roles = GetRoleNames(user);
        var key = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is missing.");
        var issuer = _configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer is missing.");
        var audiences = GetJwtAudiences();
        var token = _jwtService.BuildToken(key, issuer, audiences, user.UserId.ToString(), user.Email, roles);
        var refreshToken = _jwtService.GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

        await _userRepository.SaveChangesAsync();

        if (createdUser)
        {
            await _eventPublisher.PublishAsync(new UserRegisteredEvent
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                RegisteredAtUtc = DateTime.UtcNow
            });
        }

        return CreateAuthResponse(user, roles, token, refreshToken);
    }

    private static GoogleUserInfoDto MapGoogleUserInfo(GoogleJsonWebSignature.Payload payload)
    {
        return new GoogleUserInfoDto
        {
            Sub = payload.Subject ?? string.Empty,
            Email = payload.Email ?? string.Empty,
            Name = payload.Name ?? payload.Email ?? string.Empty,
            EmailVerified = payload.EmailVerified is true,
            Picture = payload.Picture ?? string.Empty
        };
    }

    private async Task<GoogleTokenResponse> ExchangeGoogleAuthorizationCodeAsync(string code)
    {
        var clientId = _configuration["Google:ClientId"] ?? throw new InvalidOperationException("Google:ClientId is missing.");
        var clientSecret = _configuration["Google:ClientSecret"] ?? throw new InvalidOperationException("Google:ClientSecret is missing.");
        var redirectUri = _configuration["Google:RedirectUri"] ?? throw new InvalidOperationException("Google:RedirectUri is missing.");

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://oauth2.googleapis.com/token")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["code"] = code,
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
                ["redirect_uri"] = redirectUri,
                ["grant_type"] = "authorization_code"
            })
        };

        using var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            throw new UnauthorizedException("Failed to exchange Google authorization code.");
        }

        var tokenResponse = await response.Content.ReadFromJsonAsync<GoogleTokenResponse>();
        if (tokenResponse is null)
        {
            throw new UnauthorizedException("Google token exchange returned an empty response.");
        }

        return tokenResponse;
    }

    private static AuthResponseDto CreateAuthResponse(User user, IReadOnlyCollection<string> roles, string token, string refreshToken)
    {
        return new AuthResponseDto
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(60),
            Roles = roles.ToList()
        };
    }

    private string[] GetRoleNames(User user)
    {
        var cacheKey = GetUserRoleCacheKey(user.UserId);
        if (_memoryCache.TryGetValue(cacheKey, out string[]? cachedRoles) && cachedRoles is not null)
        {
            return cachedRoles;
        }

        var roles = new List<string>();

        foreach (var userRole in user.UserRoles)
        {
            var roleName = userRole.Role?.RoleName;
            if (!string.IsNullOrWhiteSpace(roleName) && !roles.Contains(roleName))
            {
                roles.Add(roleName);
            }
        }

        var roleArray = roles.ToArray();
        _memoryCache.Set(cacheKey, roleArray, TimeSpan.FromMinutes(10));
        return roleArray;
    }

    private static string GetUserRoleCacheKey(Guid userId)
    {
        return $"user_role_{userId}";
    }

    private string[] GetJwtAudiences()
    {
        var audiences = new List<string>();

        AddAudience("Jwt:Aud1");
        AddAudience("Jwt:Aud2");
        AddAudience("Jwt:Aud3");
        AddAudience("Jwt:Aud4");
        AddAudience("Jwt:Aud5");

        return audiences.ToArray();

        void AddAudience(string key)
        {
            var audience = _configuration[key];
            if (!string.IsNullOrWhiteSpace(audience))
            {
                audiences.Add(audience);
            }
        }
    }

    private sealed class GoogleTokenResponse
    {
        [JsonPropertyName("id_token")]
        public string? IdToken { get; set; }
    }
}