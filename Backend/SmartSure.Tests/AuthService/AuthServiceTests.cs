using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Moq;
using SmartSure.IdentityService.DTOs;
using SmartSure.IdentityService.Helpers;
using SmartSure.IdentityService.Models;
using SmartSure.IdentityService.Repositories;
using SmartSure.IdentityService.Services;
using SmartSure.Shared.Exceptions;

namespace SmartSure.Tests.AuthService;

[TestFixture]
[Category("AuthService")]
public class AuthServiceTests
{
    private Mock<IUserRepository> _userRepo = null!;
    private Mock<IRoleRepository> _roleRepo = null!;
    private Mock<IJwtService> _jwtService = null!;
    private Mock<IEmailService> _emailService = null!;
    private Mock<IUserRegisteredEventPublisher> _eventPublisher = null!;
    private IMemoryCache _cache = null!;
    private IConfiguration _config = null!;
    private SmartSure.IdentityService.Services.AuthService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _userRepo       = new Mock<IUserRepository>();
        _roleRepo       = new Mock<IRoleRepository>();
        _jwtService     = new Mock<IJwtService>();
        _emailService   = new Mock<IEmailService>();
        _eventPublisher = new Mock<IUserRegisteredEventPublisher>();
        _cache          = new MemoryCache(new MemoryCacheOptions());

        var configValues = new Dictionary<string, string?>
        {
            ["Jwt:Key"]    = "test-key-that-is-long-enough-32ch",
            ["Jwt:Issuer"] = "TestIssuer",
            ["Jwt:Aud1"]   = "TestAudience"
        };
        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues)
            .Build();

        _sut = new SmartSure.IdentityService.Services.AuthService(
            _config, _cache, _userRepo.Object, _roleRepo.Object,
            _jwtService.Object, _emailService.Object, _eventPublisher.Object);
    }

    [TearDown]
    public void TearDown() => _cache.Dispose();

    // ── Register ──────────────────────────────────────────────────────────

    [Test]
    [Description("Register should throw ValidationException when full name is too short")]
    public void Register_ShortFullName_ThrowsValidationException()
    {
        var dto = new RegisterDto { FullName = "A", Email = "test@test.com", Password = "Valid1@pass" };
        Assert.ThrowsAsync<ValidationException>(() => _sut.RegisterAsync(dto));
    }

    [Test]
    [Description("Register should throw ValidationException when email is invalid")]
    public void Register_InvalidEmail_ThrowsValidationException()
    {
        var dto = new RegisterDto { FullName = "John Doe", Email = "not-an-email", Password = "Valid1@pass" };
        Assert.ThrowsAsync<ValidationException>(() => _sut.RegisterAsync(dto));
    }

    [Test]
    [Description("Register should throw ValidationException when password is too short")]
    public void Register_ShortPassword_ThrowsValidationException()
    {
        var dto = new RegisterDto { FullName = "John Doe", Email = "john@test.com", Password = "abc" };
        Assert.ThrowsAsync<ValidationException>(() => _sut.RegisterAsync(dto));
    }

    [Test]
    [Description("Register should throw ValidationException when password lacks special character")]
    public void Register_WeakPassword_ThrowsValidationException()
    {
        var dto = new RegisterDto { FullName = "John Doe", Email = "john@test.com", Password = "Password123" };
        Assert.ThrowsAsync<ValidationException>(() => _sut.RegisterAsync(dto));
    }

    [Test]
    [Description("Register should throw ConflictException when email already exists")]
    public async Task Register_DuplicateEmail_ThrowsConflictException()
    {
        _userRepo.Setup(r => r.GetByEmailAsync("john@test.com"))
                 .ReturnsAsync(new User { UserId = Guid.NewGuid(), Email = "john@test.com" });

        var dto = new RegisterDto { FullName = "John Doe", Email = "john@test.com", Password = "Valid1@pass!" };
        Assert.ThrowsAsync<ConflictException>(() => _sut.RegisterAsync(dto));
        await Task.CompletedTask;
    }

    [Test]
    [Description("Register with valid data should send OTP email and return success message")]
    public async Task Register_ValidData_SendsOtpAndReturnsMessage()
    {
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        _emailService.Setup(e => e.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                     .Returns(Task.CompletedTask);

        var dto = new RegisterDto { FullName = "John Doe", Email = "john@test.com", Password = "Valid1@pass!" };
        var result = await _sut.RegisterAsync(dto);

        Assert.That(result.Message, Is.Not.Null.And.Not.Empty);
        _emailService.Verify(e => e.SendAsync("john@test.com", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    // ── Login ─────────────────────────────────────────────────────────────

    [Test]
    [Description("Login should throw ValidationException when email is empty")]
    public void Login_EmptyEmail_ThrowsValidationException()
    {
        var dto = new LoginDto { Email = "", Password = "somepass" };
        Assert.ThrowsAsync<ValidationException>(() => _sut.LoginAsync(dto));
    }

    [Test]
    [Description("Login should throw ValidationException when password is empty")]
    public void Login_EmptyPassword_ThrowsValidationException()
    {
        var dto = new LoginDto { Email = "john@test.com", Password = "" };
        Assert.ThrowsAsync<ValidationException>(() => _sut.LoginAsync(dto));
    }

    [Test]
    [Description("Login should throw UnauthorizedException when user does not exist")]
    public void Login_UserNotFound_ThrowsUnauthorizedException()
    {
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        var dto = new LoginDto { Email = "nobody@test.com", Password = "Valid1@pass!" };
        Assert.ThrowsAsync<UnauthorizedException>(() => _sut.LoginAsync(dto));
    }

    [Test]
    [Description("Login should throw UnauthorizedException when account is deactivated")]
    public void Login_DeactivatedAccount_ThrowsUnauthorizedException()
    {
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
                 .ReturnsAsync(new User { UserId = Guid.NewGuid(), Email = "john@test.com", IsActive = false });

        var dto = new LoginDto { Email = "john@test.com", Password = "Valid1@pass!" };
        Assert.ThrowsAsync<UnauthorizedException>(() => _sut.LoginAsync(dto));
    }

    [Test]
    [Description("Login should throw UnauthorizedException when password is wrong")]
    public void Login_WrongPassword_ThrowsUnauthorizedException()
    {
        var user = new User
        {
            UserId   = Guid.NewGuid(),
            Email    = "john@test.com",
            IsActive = true,
            Password = new Password { PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPass1@") },
            UserRoles = new List<UserRole>()
        };
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);

        var dto = new LoginDto { Email = "john@test.com", Password = "WrongPass1@" };
        Assert.ThrowsAsync<UnauthorizedException>(() => _sut.LoginAsync(dto));
    }

    // ── Password Reset ────────────────────────────────────────────────────

    [Test]
    [Description("RequestPasswordReset should silently succeed even when email does not exist (prevents enumeration)")]
    public async Task RequestPasswordReset_UnknownEmail_SilentlySucceeds()
    {
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

        // Should NOT throw — user enumeration prevention
        await _sut.RequestPasswordResetAsync(new ForgotPasswordDto { Email = "ghost@test.com" });

        _emailService.Verify(e => e.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    [Description("ResetPassword should throw ValidationException when new password is too weak")]
    public void ResetPassword_WeakNewPassword_ThrowsValidationException()
    {
        var dto = new ResetPasswordDto { Email = "john@test.com", Token = "123456", NewPassword = "weak" };
        Assert.ThrowsAsync<ValidationException>(() => _sut.ResetPasswordAsync(dto));
    }
}
