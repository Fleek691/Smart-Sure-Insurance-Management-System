using Microsoft.Extensions.Caching.Memory;
using SmartSure.IdentityService.Helpers;
using SmartSure.IdentityService.DTOs;
using SmartSure.IdentityService.Models;
using SmartSure.IdentityService.Repositories;
using SmartSure.Shared.Constants;
using SmartSure.Shared.Events;
using SmartSure.Shared.Exceptions;

namespace SmartSure.IdentityService.Services;

/// <summary>
/// Core authentication logic: OTP-based registration, login, token refresh, and password reset.
/// </summary>
public class AuthService : IAuthService
{
    private const int RegistrationOtpMinutes = 10;
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _memoryCache;
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IJwtService _jwtService;
    private readonly IEmailService _emailService;
    private readonly IUserRegisteredEventPublisher _eventPublisher;

    public AuthService(
        IConfiguration configuration,
        IMemoryCache memoryCache,
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IJwtService jwtService,
        IEmailService emailService,
        IUserRegisteredEventPublisher eventPublisher)
    {
        _configuration = configuration;
        _memoryCache = memoryCache;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _jwtService = jwtService;
        _emailService = emailService;
        _eventPublisher = eventPublisher;
    }

    /// <summary>
    /// Validates input, checks for duplicate email, generates a 6-digit OTP,
    /// caches the pending registration, and sends the OTP via email.
    /// </summary>
    public async Task<OtpDispatchResponseDto> RegisterAsync(RegisterDto dto)
    {
        // ── Input validation (defense-in-depth) ──
        if (string.IsNullOrWhiteSpace(dto.FullName) || dto.FullName.Trim().Length < 2)
        {
            throw new ValidationException("Full name must be at least 2 characters.");
        }

        if (string.IsNullOrWhiteSpace(dto.Email) || !IsValidEmail(dto.Email))
        {
            throw new ValidationException("Please provide a valid email address.");
        }

        if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 8)
        {
            throw new ValidationException("Password must be at least 8 characters long.");
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(dto.Password,
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#])"))
        {
            throw new ValidationException("Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character (@$!%*?&#).");
        }

        var email = NormalizeEmail(dto.Email);

        var existingUser = await _userRepository.GetByEmailAsync(email);
        if (existingUser is not null)
        {
            throw new ConflictException("An account with this email already exists.");
        }

        var otp = GenerateOtp();
        var expiresAt = DateTime.UtcNow.AddMinutes(RegistrationOtpMinutes);
        CacheRegistrationOtp(email, otp, expiresAt);
        CachePendingRegistration(new PendingRegistration
        {
            FullName = dto.FullName,
            Email = email,
            Password = dto.Password,
            PhoneNumber = dto.PhoneNumber,
            Address = dto.Address
        }, expiresAt);

        await _emailService.SendAsync(email, "Your SmartSure registration OTP", $"Your 6 digit OTP is: {otp}. It expires in {RegistrationOtpMinutes} minutes.");

        return new OtpDispatchResponseDto
        {
            Message = "OTP sent to your email. Verify OTP to complete registration.",
            ExpiresAtUtc = expiresAt
        };
    }

    /// <summary>
    /// Verifies the OTP against the cached value, creates the user with a hashed password,
    /// assigns the CUSTOMER role, issues JWT + refresh token, and publishes a UserRegisteredEvent.
    /// </summary>
    public async Task<AuthResponseDto> VerifyRegistrationOtpAsync(VerifyRegistrationOtpDto dto)
    {
        var email = NormalizeEmail(dto.Email);
        var providedOtp = dto.Otp.Trim();

        var pendingRegistration = GetPendingRegistration(email)
            ?? throw new ValidationException("No pending registration found for this email.");

        var cachedOtp = GetRegistrationOtp(email)
            ?? throw new ValidationException("OTP expired. Please request a new OTP.");

        if (!string.Equals(cachedOtp, providedOtp, StringComparison.Ordinal))
        {
            throw new ValidationException("Invalid OTP.");
        }

        var existingUser = await _userRepository.GetByEmailAsync(email);
        if (existingUser is not null)
        {
            throw new ConflictException("Email already exists.");
        }

        var role = await _roleRepository.GetByNameAsync(Roles.Customer)
                   ?? new Role { RoleId = Guid.NewGuid(), RoleName = Roles.Customer };

        var user = new User
        {
            UserId = Guid.NewGuid(),
            FullName = pendingRegistration.FullName,
            Email = email,
            PhoneNumber = pendingRegistration.PhoneNumber,
            Address = pendingRegistration.Address,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            Password = new Password
            {
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(pendingRegistration.Password)
            },
            UserRoles = new List<UserRole>()
        };

        user.UserRoles.Add(new UserRole
        {
            Role = role,
            RoleId = role.RoleId
        });

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        var roles = CacheUserRoles(user.UserId, new[] { role.RoleName });
        var key = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is missing.");
        var issuer = _configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer is missing.");
        var audiences = GetJwtAudiences();
        var token = _jwtService.BuildToken(
            key,
            issuer,
            audiences,
            user.UserId.ToString(),
            user.Email,
            roles);
        var refreshToken = _jwtService.GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userRepository.SaveChangesAsync();

        ClearPendingRegistration(email);
        ClearRegistrationOtp(email);

        var response = CreateAuthResponse(user, roles, token, refreshToken);
        await _eventPublisher.PublishAsync(new UserRegisteredEvent
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            RegisteredAtUtc = DateTime.UtcNow
        });

        return response;
    }

    /// <summary>
    /// Generates a new OTP for an existing pending registration and re-sends it via email.
    /// </summary>
    public async Task<OtpDispatchResponseDto> ResendRegistrationOtpAsync(ResendRegistrationOtpDto dto)
    {
        var email = NormalizeEmail(dto.Email);
        var pendingRegistration = GetPendingRegistration(email)
            ?? throw new ValidationException("No pending registration found for this email.");

        var otp = GenerateOtp();
        var expiresAt = DateTime.UtcNow.AddMinutes(RegistrationOtpMinutes);

        CacheRegistrationOtp(email, otp, expiresAt);
        CachePendingRegistration(pendingRegistration, expiresAt);

        await _emailService.SendAsync(email, "Your SmartSure registration OTP", $"Your 6 digit OTP is: {otp}. It expires in {RegistrationOtpMinutes} minutes.");

        return new OtpDispatchResponseDto
        {
            Message = "A new OTP has been sent to your email.",
            ExpiresAtUtc = expiresAt
        };
    }

    /// <summary>
    /// Authenticates the user by verifying email/password (BCrypt), then issues a new JWT
    /// and rotates the refresh token.
    /// </summary>
    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || !IsValidEmail(dto.Email))
        {
            throw new ValidationException("Please provide a valid email address.");
        }

        if (string.IsNullOrWhiteSpace(dto.Password))
        {
            throw new ValidationException("Password is required.");
        }

        var user = await _userRepository.GetByEmailAsync(dto.Email)
                   ?? throw new UnauthorizedException("Invalid email or password.");

        if (!user.IsActive)
        {
            throw new UnauthorizedException("Your account has been deactivated. Please contact support.");
        }

        if (user.Password is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password.PasswordHash))
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        user.LastLoginAt = DateTime.UtcNow;

        var roles = GetRoleNames(user);
        var key = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is missing.");
        var issuer = _configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer is missing.");
        var audiences = GetJwtAudiences();
        var token = _jwtService.BuildToken(
            key,
            issuer,
            audiences,
            user.UserId.ToString(),
            user.Email,
            roles);
        var refreshToken = _jwtService.GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userRepository.SaveChangesAsync();
        return CreateAuthResponse(user, roles, token, refreshToken);
    }

    /// <summary>
    /// Validates the refresh token, issues a new JWT, and rotates the refresh token
    /// (old token becomes invalid).
    /// </summary>
    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto)
    {
        var user = await _userRepository.GetByRefreshTokenAsync(dto.Token)
                   ?? throw new UnauthorizedException("Invalid refresh token.");

        if (user.RefreshTokenExpiryTime is null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            throw new UnauthorizedException("Refresh token expired.");
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

        return CreateAuthResponse(user, roles, token, refreshToken);
    }

    /// <summary>
    /// Generates a password reset OTP and emails it. Intentionally silent on unknown
    /// emails to prevent user enumeration.
    /// </summary>
    public async Task RequestPasswordResetAsync(ForgotPasswordDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || !IsValidEmail(dto.Email))
        {
            throw new ValidationException("Please provide a valid email address.");
        }

        // Intentionally silent when email not found — prevents user enumeration attacks
        var user = await _userRepository.GetByEmailAsync(NormalizeEmail(dto.Email));
        if (user is null) return;

        var resetToken = System.Security.Cryptography.RandomNumberGenerator.GetInt32(0, 1000000).ToString("D6");
        user.PasswordResetTokens.Add(new PasswordResetToken
        {
            Token = resetToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            IsUsed = false
        });

        await _userRepository.SaveChangesAsync();
        await _emailService.SendAsync(user.Email, "Your SmartSure password reset OTP", $"Your 6-digit OTP is: {resetToken}. It expires in 15 minutes.");
    }

    /// <summary>
    /// Validates the reset OTP, marks it as used, and updates the user's password hash.
    /// </summary>
    public async Task ResetPasswordAsync(ResetPasswordDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || !IsValidEmail(dto.Email))
        {
            throw new ValidationException("Please provide a valid email address.");
        }

        if (string.IsNullOrWhiteSpace(dto.NewPassword) || dto.NewPassword.Length < 8)
        {
            throw new ValidationException("New password must be at least 8 characters long.");
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(dto.NewPassword,
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#])"))
        {
            throw new ValidationException("New password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character (@$!%*?&#).");
        }

        var user = await _userRepository.GetByEmailAsync(NormalizeEmail(dto.Email))
                   ?? throw new ValidationException("Invalid or expired token.");

        PasswordResetToken? token = null;
        var incomingToken = dto.Token.Trim();
        foreach (var resetToken in user.PasswordResetTokens)
        {
            if (string.Equals(resetToken.Token.Trim(), incomingToken, StringComparison.Ordinal) && !resetToken.IsUsed)
            {
                token = resetToken;
                break;
            }
        }

        if (token is null || token.ExpiresAt < DateTime.UtcNow)
        {
            throw new ValidationException("Invalid or expired token.");
        }

        token.IsUsed = true;
        if (user.Password is null)
        {
            user.Password = new Password { PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword) };
        }
        else
        {
            user.Password.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        }

        await _userRepository.SaveChangesAsync();
    }

    /// <summary>Maps a User entity to the auth response DTO with token details.</summary>
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

    /// <summary>Extracts role names from the user, using a 10-minute memory cache.</summary>
    private string[] GetRoleNames(User user)
    {
        if (_memoryCache.TryGetValue(GetUserRoleCacheKey(user.UserId), out string[]? cachedRoles) && cachedRoles is not null)
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
        CacheUserRoles(user.UserId, roleArray);
        return roleArray;
    }

    private string[] CacheUserRoles(Guid userId, IReadOnlyCollection<string> roles)
    {
        var roleArray = roles.ToArray();
        _memoryCache.Set(GetUserRoleCacheKey(userId), roleArray, TimeSpan.FromMinutes(10));
        return roleArray;
    }

    private static string GetUserRoleCacheKey(Guid userId)
    {
        return $"user_role_{userId}";
    }

    /// <summary>Reads Jwt:Aud1–Aud5 from configuration to build the multi-audience list.</summary>
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

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email.Trim());
            return addr.Address == email.Trim();
        }
        catch
        {
            return false;
        }
    }

    private static string GenerateOtp()
    {
        return System.Security.Cryptography.RandomNumberGenerator.GetInt32(0, 1000000).ToString("D6");
    }

    private void CacheRegistrationOtp(string email, string otp, DateTime expiresAtUtc)
    {
        var ttl = expiresAtUtc - DateTime.UtcNow;
        _memoryCache.Set(GetRegistrationOtpCacheKey(email), otp, ttl);
    }

    private string? GetRegistrationOtp(string email)
    {
        if (_memoryCache.TryGetValue(GetRegistrationOtpCacheKey(email), out string? otp) && !string.IsNullOrWhiteSpace(otp))
        {
            return otp;
        }

        return null;
    }

    private void ClearRegistrationOtp(string email)
    {
        _memoryCache.Remove(GetRegistrationOtpCacheKey(email));
    }

    private void CachePendingRegistration(PendingRegistration pending, DateTime expiresAtUtc)
    {
        var ttl = expiresAtUtc - DateTime.UtcNow;
        _memoryCache.Set(GetPendingRegistrationCacheKey(pending.Email), pending, ttl);
    }

    private PendingRegistration? GetPendingRegistration(string email)
    {
        if (_memoryCache.TryGetValue(GetPendingRegistrationCacheKey(email), out PendingRegistration? pending) && pending is not null)
        {
            return pending;
        }

        return null;
    }

    private void ClearPendingRegistration(string email)
    {
        _memoryCache.Remove(GetPendingRegistrationCacheKey(email));
    }

    private static string GetRegistrationOtpCacheKey(string email)
    {
        return $"registration_otp_{email}";
    }

    private static string GetPendingRegistrationCacheKey(string email)
    {
        return $"registration_pending_{email}";
    }

    /// <summary>Temporary record stored in IMemoryCache during the OTP verification window.</summary>
    private sealed class PendingRegistration
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
    }

}
