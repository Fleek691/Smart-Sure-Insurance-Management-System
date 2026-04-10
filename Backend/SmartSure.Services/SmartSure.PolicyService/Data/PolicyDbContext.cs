using Microsoft.EntityFrameworkCore;
using SmartSure.PolicyService.Models;

namespace SmartSure.PolicyService.Data;

public class PolicyDbContext(DbContextOptions<PolicyDbContext> options) : DbContext(options)
{
    public DbSet<InsuranceType> InsuranceTypes => Set<InsuranceType>();
    public DbSet<InsuranceSubType> InsuranceSubTypes => Set<InsuranceSubType>();
    public DbSet<Policy> Policies => Set<Policy>();
    public DbSet<PolicyDetail> PolicyDetails => Set<PolicyDetail>();
    public DbSet<VehicleDetail> VehicleDetails => Set<VehicleDetail>();
    public DbSet<HomeDetail> HomeDetails => Set<HomeDetail>();
    public DbSet<Premium> Premiums => Set<Premium>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InsuranceType>(entity =>
        {
            entity.HasKey(x => x.TypeId);
            entity.Property(x => x.TypeId).ValueGeneratedOnAdd();
            entity.Property(x => x.TypeName).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => x.TypeName).IsUnique();
        });

        modelBuilder.Entity<InsuranceSubType>(entity =>
        {
            entity.HasKey(x => x.SubTypeId);
            entity.Property(x => x.SubTypeId).ValueGeneratedOnAdd();
            entity.Property(x => x.SubTypeName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.BasePremium).HasColumnType("decimal(18,2)");
            entity.HasOne(x => x.Type)
                .WithMany(x => x.SubTypes)
                .HasForeignKey(x => x.TypeId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.TypeId, x.SubTypeName }).IsUnique();
        });

        modelBuilder.Entity<Policy>(entity =>
        {
            entity.HasKey(x => x.PolicyId);
            entity.Property(x => x.PolicyId).HasDefaultValueSql("NEWID()");
            entity.Property(x => x.PolicyNumber).HasMaxLength(25).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(20).IsRequired();
            entity.Property(x => x.CoverageAmount).HasColumnType("decimal(14,2)");
            entity.Property(x => x.MonthlyPremium).HasColumnType("decimal(10,2)");
            entity.HasIndex(x => x.PolicyNumber).IsUnique();
            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => x.Status);
            entity.HasOne(x => x.Type)
                .WithMany(x => x.Policies)
                .HasForeignKey(x => x.TypeId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.SubType)
                .WithMany(x => x.Policies)
                .HasForeignKey(x => x.SubTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PolicyDetail>(entity =>
        {
            entity.HasKey(x => x.PolicyDetailId);
            entity.Property(x => x.PolicyDetailId).HasDefaultValueSql("NEWID()");
            entity.Property(x => x.Details).HasColumnType("nvarchar(max)").IsRequired();
            entity.HasOne(x => x.Policy)
                .WithOne(x => x.PolicyDetail)
                .HasForeignKey<PolicyDetail>(x => x.PolicyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<VehicleDetail>(entity =>
        {
            entity.HasKey(x => x.VehicleId);
            entity.Property(x => x.VehicleId).HasDefaultValueSql("NEWID()");
            entity.Property(x => x.VehicleNumber).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Model).HasMaxLength(100).IsRequired();
            entity.HasOne(x => x.Policy)
                .WithOne(x => x.VehicleDetail)
                .HasForeignKey<VehicleDetail>(x => x.PolicyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<HomeDetail>(entity =>
        {
            entity.HasKey(x => x.HomeId);
            entity.Property(x => x.HomeId).HasDefaultValueSql("NEWID()");
            entity.Property(x => x.Address).HasMaxLength(300).IsRequired();
            entity.Property(x => x.ConstructionType).HasMaxLength(50);
            entity.HasOne(x => x.Policy)
                .WithOne(x => x.HomeDetail)
                .HasForeignKey<HomeDetail>(x => x.PolicyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Premium>(entity =>
        {
            entity.HasKey(x => x.PremiumId);
            entity.Property(x => x.PremiumId).HasDefaultValueSql("NEWID()");
            entity.Property(x => x.Amount).HasColumnType("decimal(10,2)");
            entity.Property(x => x.Status).HasMaxLength(20).IsRequired();
            entity.HasOne(x => x.Policy)
                .WithMany(x => x.Premiums)
                .HasForeignKey(x => x.PolicyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(x => x.PaymentId);
            entity.Property(x => x.PaymentId).HasDefaultValueSql("NEWID()");
            entity.Property(x => x.Amount).HasColumnType("decimal(10,2)");
            entity.Property(x => x.PaymentMethod).HasMaxLength(50).IsRequired();
            entity.Property(x => x.TransactionRef).HasMaxLength(100);
            entity.Property(x => x.Status).HasMaxLength(20).IsRequired();
            entity.HasOne(x => x.Policy)
                .WithMany(x => x.Payments)
                .HasForeignKey(x => x.PolicyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        SeedInsuranceData(modelBuilder);
    }

    private static void SeedInsuranceData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InsuranceType>().HasData(
            new InsuranceType { TypeId = 1, TypeName = "Vehicle" },
            new InsuranceType { TypeId = 2, TypeName = "Home" }
        );

        modelBuilder.Entity<InsuranceSubType>().HasData(
            new InsuranceSubType { SubTypeId = 101, TypeId = 1, SubTypeName = "Mahindra", BasePremium = 15000m },
            new InsuranceSubType { SubTypeId = 102, TypeId = 1, SubTypeName = "Maruti Suzuki", BasePremium = 12000m },
            new InsuranceSubType { SubTypeId = 103, TypeId = 1, SubTypeName = "Hyundai", BasePremium = 13000m },
            new InsuranceSubType { SubTypeId = 104, TypeId = 1, SubTypeName = "Honda", BasePremium = 14000m },
            new InsuranceSubType { SubTypeId = 105, TypeId = 1, SubTypeName = "Tata Motors", BasePremium = 13500m },
            new InsuranceSubType { SubTypeId = 106, TypeId = 1, SubTypeName = "Toyota", BasePremium = 16000m },
            new InsuranceSubType { SubTypeId = 107, TypeId = 1, SubTypeName = "Kia", BasePremium = 14500m },
            new InsuranceSubType { SubTypeId = 108, TypeId = 1, SubTypeName = "Volkswagen", BasePremium = 17000m },
            new InsuranceSubType { SubTypeId = 109, TypeId = 1, SubTypeName = "Skoda", BasePremium = 16500m },
            new InsuranceSubType { SubTypeId = 110, TypeId = 1, SubTypeName = "Renault", BasePremium = 13000m },
            new InsuranceSubType { SubTypeId = 111, TypeId = 1, SubTypeName = "Nissan", BasePremium = 14000m },
            new InsuranceSubType { SubTypeId = 112, TypeId = 1, SubTypeName = "Ford", BasePremium = 15500m },
            new InsuranceSubType { SubTypeId = 113, TypeId = 1, SubTypeName = "MG Motor", BasePremium = 15000m },
            new InsuranceSubType { SubTypeId = 114, TypeId = 1, SubTypeName = "Jeep", BasePremium = 18000m },
            new InsuranceSubType { SubTypeId = 115, TypeId = 1, SubTypeName = "BMW", BasePremium = 25000m },
            new InsuranceSubType { SubTypeId = 116, TypeId = 1, SubTypeName = "Mercedes-Benz", BasePremium = 28000m },
            new InsuranceSubType { SubTypeId = 117, TypeId = 1, SubTypeName = "Audi", BasePremium = 26000m },
            new InsuranceSubType { SubTypeId = 118, TypeId = 1, SubTypeName = "Volvo", BasePremium = 24000m },
            new InsuranceSubType { SubTypeId = 201, TypeId = 2, SubTypeName = "Apartment", BasePremium = 8000m },
            new InsuranceSubType { SubTypeId = 202, TypeId = 2, SubTypeName = "Independent House", BasePremium = 12000m },
            new InsuranceSubType { SubTypeId = 203, TypeId = 2, SubTypeName = "Villa", BasePremium = 18000m },
            new InsuranceSubType { SubTypeId = 204, TypeId = 2, SubTypeName = "Bungalow", BasePremium = 15000m },
            new InsuranceSubType { SubTypeId = 205, TypeId = 2, SubTypeName = "Penthouse", BasePremium = 20000m },
            new InsuranceSubType { SubTypeId = 206, TypeId = 2, SubTypeName = "Studio Apartment", BasePremium = 6000m },
            new InsuranceSubType { SubTypeId = 207, TypeId = 2, SubTypeName = "Duplex", BasePremium = 14000m },
            new InsuranceSubType { SubTypeId = 208, TypeId = 2, SubTypeName = "Farmhouse", BasePremium = 16000m }
        );
    }
}