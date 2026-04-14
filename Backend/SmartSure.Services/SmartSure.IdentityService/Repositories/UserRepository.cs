using Microsoft.EntityFrameworkCore;
using SmartSure.IdentityService.Data;
using SmartSure.IdentityService.Models;

namespace SmartSure.IdentityService.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IUserRepository"/>.
/// All lookups load all users into memory first and then filter in-process
/// to guarantee case-insensitive email comparisons across database collations.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly IdentityDbContext _context;

    public UserRepository(IdentityDbContext context)
    {
        _context = context;
    }

    // ── Public interface ──────────────────────────────────────────────────────

    public Task<User?> GetByEmailAsync(string email)
    {
        return GetByEmailInternalAsync(email);
    }

    public Task<User?> GetByIdAsync(Guid userId)
    {
        return GetByIdInternalAsync(userId);
    }

    public Task<User?> GetByRefreshTokenAsync(string refreshToken)
    {
        return GetByRefreshTokenInternalAsync(refreshToken);
    }

    public Task<List<User>> GetAllAsync()
    {
        return GetAllInternalAsync();
    }

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
    }

    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }

    // ── Private implementations ───────────────────────────────────────────────

    /// <summary>
    /// Loads all users with their password and roles, then finds the one
    /// matching <paramref name="email"/> using a case-insensitive comparison.
    /// </summary>
    private async Task<User?> GetByEmailInternalAsync(string email)
    {
        var users = await _context.Users
            .Include(nameof(User.Password))
            .Include(nameof(User.PasswordResetTokens))
            .Include($"{nameof(User.UserRoles)}.{nameof(UserRole.Role)}")
            .ToListAsync();

        foreach (var user in users)
        {
            if (string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
            {
                return user;
            }
        }

        return null;
    }

    /// <summary>Loads all users and returns the one matching <paramref name="userId"/>.</summary>
    private async Task<User?> GetByIdInternalAsync(Guid userId)
    {
        var users = await _context.Users
            .Include(nameof(User.Password))
            .Include($"{nameof(User.UserRoles)}.{nameof(UserRole.Role)}")
            .ToListAsync();

        foreach (var user in users)
        {
            if (user.UserId == userId)
            {
                return user;
            }
        }

        return null;
    }

    /// <summary>
    /// Loads all users and returns the one whose refresh token matches exactly
    /// (ordinal comparison — tokens are case-sensitive base64 strings).
    /// </summary>
    private async Task<User?> GetByRefreshTokenInternalAsync(string refreshToken)
    {
        var users = await _context.Users
            .Include(nameof(User.Password))
            .Include($"{nameof(User.UserRoles)}.{nameof(UserRole.Role)}")
            .ToListAsync();

        foreach (var user in users)
        {
            if (string.Equals(user.RefreshToken, refreshToken, StringComparison.Ordinal))
            {
                return user;
            }
        }

        return null;
    }

    /// <summary>Returns all users with their roles eagerly loaded.</summary>
    private async Task<List<User>> GetAllInternalAsync()
    {
        var users = await _context.Users
            .Include($"{nameof(User.UserRoles)}.{nameof(UserRole.Role)}")
            .ToListAsync();

        return users;
    }
}
