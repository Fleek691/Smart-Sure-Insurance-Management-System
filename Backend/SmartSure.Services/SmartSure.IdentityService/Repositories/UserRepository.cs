using Microsoft.EntityFrameworkCore;
using SmartSure.IdentityService.Data;
using SmartSure.IdentityService.Models;

namespace SmartSure.IdentityService.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IdentityDbContext _context;

    public UserRepository(IdentityDbContext context)
    {
        _context = context;
    }

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

    private async Task<List<User>> GetAllInternalAsync()
    {
        var users = await _context.Users
            .Include($"{nameof(User.UserRoles)}.{nameof(UserRole.Role)}")
            .ToListAsync();

        return users;
    }
}
