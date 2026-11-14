using Microsoft.EntityFrameworkCore;
using OnlineStore.Domain.Entities;

namespace OnlineStore.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<PasswordResetOtp> PasswordResetOtps => Set<PasswordResetOtp>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users", table =>
            {
                table.HasCheckConstraint(
                    "CK_Users_Role",
                    "[Role] IN ('ADMIN', 'CUSTOMER')");
            });

            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasMaxLength(450);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(256).IsRequired();
            entity.Property(x => x.HashedPassword).IsRequired();
            entity.Property(x => x.Role).HasMaxLength(8).HasDefaultValue(UserRoles.Customer).IsRequired();
            entity.HasIndex(x => x.Email).IsUnique();
        });

        modelBuilder.Entity<PasswordResetOtp>(entity =>
        {
            entity.ToTable("PasswordResetOtps", table =>
            {
                table.HasCheckConstraint(
                    "CK_PasswordResetOtps_Expiration",
                    "[ExpiresAtUtc] > [CreatedAtUtc]");
                table.HasCheckConstraint(
                    "CK_PasswordResetOtps_UsedRequiresVerified",
                    "[IsUsed] = 0 OR [IsVerified] = 1");
            });

            entity.HasKey(x => x.Id);
            entity.Property(x => x.UserId).HasMaxLength(450).IsRequired();
            entity.Property(x => x.OtpNum).HasMaxLength(10).IsRequired();
            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.UserId, x.IsUsed, x.IsVerified, x.ExpiresAtUtc });
        });

        modelBuilder.Entity<Brand>(entity =>
        {
            entity.ToTable("Brands");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.NameAr).HasMaxLength(200).IsRequired();
            entity.Property(x => x.NameEn).HasMaxLength(200).IsRequired();
            entity.Property(x => x.DisplayImageUrl).HasMaxLength(2048);
            entity.HasIndex(x => x.NameEn).IsUnique();
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Categories");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.NameAr).HasMaxLength(200).IsRequired();
            entity.Property(x => x.NameEn).HasMaxLength(200).IsRequired();
            entity.Property(x => x.DisplayImageUrl).HasMaxLength(2048);
            entity.Property(x => x.IsActive).HasDefaultValue(true).IsRequired();
            entity.HasIndex(x => x.NameEn).IsUnique();
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products", table =>
            {
                table.HasCheckConstraint("CK_Products_Price_Positive", "[Price] > 0");
                table.HasCheckConstraint("CK_Products_Qty_NonNegative", "[Qty] >= 0");
                table.HasCheckConstraint(
                    "CK_Products_StockStatus",
                    "[StockStatus] IN ('InStock', 'OutOfStock')");
            });

            entity.HasKey(x => x.Id);
            entity.Property(x => x.ItemNameAr).HasMaxLength(250).IsRequired();
            entity.Property(x => x.ItemNameEn).HasMaxLength(250).IsRequired();
            entity.Property(x => x.Price).HasPrecision(18, 2).IsRequired();
            entity.Property(x => x.StockStatus).HasMaxLength(20).IsRequired();
            entity.Property(x => x.PrimaryImageUrl).HasMaxLength(2048);
            entity.Property(x => x.Thumb1Url).HasMaxLength(2048);
            entity.Property(x => x.Thumb2Url).HasMaxLength(2048);
            entity.Property(x => x.Thumb3Url).HasMaxLength(2048);
            entity.Property(x => x.Thumb4Url).HasMaxLength(2048);
            entity.Property(x => x.DescriptionAr).HasMaxLength(4000);
            entity.Property(x => x.DescriptionEn).HasMaxLength(4000);
            entity.HasOne(x => x.Brand)
                .WithMany()
                .HasForeignKey(x => x.BrandId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Category)
                .WithMany()
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.IsActive, x.Id });
        });
    }
}
