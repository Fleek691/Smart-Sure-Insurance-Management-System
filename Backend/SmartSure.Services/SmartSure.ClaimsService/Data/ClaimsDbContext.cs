using Microsoft.EntityFrameworkCore;
using SmartSure.ClaimsService.Models;

namespace SmartSure.ClaimsService.Data;

public class ClaimsDbContext(DbContextOptions<ClaimsDbContext> options) : DbContext(options)
{
    public DbSet<Claim> Claims => Set<Claim>();
    public DbSet<ClaimDocument> ClaimDocuments => Set<ClaimDocument>();
    public DbSet<ClaimStatusHistory> ClaimStatusHistory => Set<ClaimStatusHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Claim>(entity =>
        {
            entity.HasKey(x => x.ClaimId);
            entity.Property(x => x.ClaimId).HasDefaultValueSql("NEWID()");
            entity.Property(x => x.ClaimNumber).HasMaxLength(25).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.ClaimAmount).HasColumnType("decimal(14,2)");
            entity.Property(x => x.Status).HasMaxLength(30).IsRequired();
            entity.Property(x => x.AdminNote).HasMaxLength(500);
            entity.HasIndex(x => x.ClaimNumber).IsUnique();
            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => x.PolicyId);
            entity.HasIndex(x => x.Status);
        });

        modelBuilder.Entity<ClaimDocument>(entity =>
        {
            entity.HasKey(x => x.DocId);
            entity.Property(x => x.DocId).HasDefaultValueSql("NEWID()");
            entity.Property(x => x.FileName).HasMaxLength(255).IsRequired();
            entity.Property(x => x.MegaNzFileId).HasMaxLength(500).IsRequired();
            entity.Property(x => x.FileUrl).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.FileType).HasMaxLength(50).IsRequired();
            entity.HasOne(x => x.Claim)
                .WithMany(x => x.Documents)
                .HasForeignKey(x => x.ClaimId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ClaimStatusHistory>(entity =>
        {
            entity.HasKey(x => x.HistoryId);
            entity.Property(x => x.HistoryId).HasDefaultValueSql("NEWID()");
            entity.Property(x => x.OldStatus).HasMaxLength(30).IsRequired();
            entity.Property(x => x.NewStatus).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Note).HasMaxLength(300);
            entity.HasOne(x => x.Claim)
                .WithMany(x => x.StatusHistory)
                .HasForeignKey(x => x.ClaimId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}