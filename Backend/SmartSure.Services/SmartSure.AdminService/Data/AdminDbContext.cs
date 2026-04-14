using Microsoft.EntityFrameworkCore;
using SmartSure.AdminService.Models;

namespace SmartSure.AdminService.Data;

/// <summary>
/// EF Core DbContext for the Admin service (audit logs, reports).
/// </summary>
public class AdminDbContext(DbContextOptions<AdminDbContext> options) : DbContext(options)
{
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Report> Reports => Set<Report>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(x => x.LogId);
            entity.Property(x => x.LogId).HasDefaultValueSql("NEWID()");
            entity.Property(x => x.Action).HasMaxLength(100).IsRequired();
            entity.Property(x => x.EntityType).HasMaxLength(50).IsRequired();
            entity.Property(x => x.EntityId).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Details).HasColumnType("nvarchar(max)");
            entity.Property(x => x.TimeStamp).IsRequired();
            entity.HasIndex(x => x.TimeStamp);
            entity.HasIndex(x => x.EntityType);
            entity.HasIndex(x => x.Action);
            entity.HasIndex(x => x.UserId);
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(x => x.ReportId);
            entity.Property(x => x.ReportId).HasDefaultValueSql("NEWID()");
            entity.Property(x => x.ReportType).HasMaxLength(50).IsRequired();
            entity.Property(x => x.GeneratedDate).IsRequired();
            entity.Property(x => x.DataJson).HasColumnType("nvarchar(max)").IsRequired();
            entity.HasIndex(x => x.ReportType);
            entity.HasIndex(x => x.GeneratedDate);
            entity.HasIndex(x => x.UserId);
        });
    }
}
