using Microsoft.EntityFrameworkCore;
using SmartSure.IdentityService.Models;
using SmartSure.Shared.Constants;

namespace SmartSure.IdentityService.Data;

/// <summary>
/// EF Core DbContext for the Identity service (users, passwords, roles).
/// </summary>
public class IdentityDbContext : DbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Password> Passwords { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

    /// <summary>
    /// Configures entity relationships, indexes, and seeds default roles.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(nameof(User.Email))
            .IsUnique();

        modelBuilder.Entity<Password>()
            .HasKey(nameof(Password.PassId));

        modelBuilder.Entity<Password>()
            .HasOne(password => password.User)
            .WithOne(user => user.Password)
            .HasForeignKey<Password>(password => password.UserId);

        modelBuilder.Entity<PasswordResetToken>()
            .HasOne(token => token.User)
            .WithMany(user => user.PasswordResetTokens)
            .HasForeignKey(token => token.UserId);

        modelBuilder.Entity<Role>()
            .HasIndex(nameof(Role.RoleName))
            .IsUnique();

        modelBuilder.Entity<UserRole>()
            .HasOne(typeof(User), nameof(UserRole.User))
            .WithMany(nameof(User.UserRoles))
            .HasForeignKey(nameof(UserRole.UserId));

        modelBuilder.Entity<UserRole>()
            .HasOne(typeof(Role), nameof(UserRole.Role))
            .WithMany(nameof(Role.UserRoles))
            .HasForeignKey(nameof(UserRole.RoleId));

        modelBuilder.Entity<Role>()
            .HasData(
                new Role { RoleId = new Guid("11111111-1111-1111-1111-111111111111"), RoleName = SmartSure.Shared.Constants.Roles.Customer },
                new Role { RoleId = new Guid("22222222-2222-2222-2222-222222222222"), RoleName = SmartSure.Shared.Constants.Roles.Admin });
    }
}