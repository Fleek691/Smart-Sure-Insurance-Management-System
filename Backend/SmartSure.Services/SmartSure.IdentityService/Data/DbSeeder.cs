using Microsoft.EntityFrameworkCore;
using SmartSure.IdentityService.Models;

namespace SmartSure.IdentityService.Data;

/// <summary>
/// Seeds the database with an initial admin user on startup.
/// Runs idempotently — skips if admin already exists.
/// </summary>
public static class DbSeeder
{
    private static readonly Guid AdminUserId = new("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA");
    private static readonly Guid AdminRoleId = new("22222222-2222-2222-2222-222222222222");

    public static async Task SeedAsync(IdentityDbContext context, IConfiguration configuration, ILogger logger)
    {
        try
        {
            // Ensure DB is created / migrated
            await context.Database.MigrateAsync();

            // Check if admin user already exists
            var adminExists = await context.Users.AnyAsync(u => u.UserId == AdminUserId);
            if (adminExists)
            {
                logger.LogInformation("Admin user already exists — skipping seed.");
                return;
            }

            var adminPassword = configuration["AdminSettings:Password"] ?? "Admin@123";

            // Create admin user
            var adminUser = new User
            {
                UserId = AdminUserId,
                FullName = "System Admin",
                Email = "admin@smartsure.com",
                PhoneNumber = "+27000000000",
                Address = "SmartSure HQ, Johannesburg",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(adminUser);
            await context.SaveChangesAsync();

            // Create password (BCrypt hashed)
            var password = new Password
            {
                PassId = Guid.NewGuid(),
                UserId = AdminUserId,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword)
            };

            context.Passwords.Add(password);
            await context.SaveChangesAsync();

            // Assign ADMIN role
            var userRole = new UserRole
            {
                Id = Guid.NewGuid(),
                UserId = AdminUserId,
                RoleId = AdminRoleId
            };

            context.UserRoles.Add(userRole);
            await context.SaveChangesAsync();

            logger.LogInformation("Seeded admin user: admin@smartsure.com / {Password}", adminPassword);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error seeding admin user");
        }
    }
}
