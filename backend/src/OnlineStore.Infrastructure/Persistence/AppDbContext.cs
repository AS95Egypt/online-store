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
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Governorate> Governorates => Set<Governorate>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderStatusHistory> OrderStatusHistory => Set<OrderStatusHistory>();

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
            entity.Property(x => x.ShippingFullName).HasMaxLength(200);
            entity.Property(x => x.ShippingPhone).HasMaxLength(30);
            entity.Property(x => x.ShippingCity).HasMaxLength(150);
            entity.Property(x => x.ShippingDistrict).HasMaxLength(150);
            entity.Property(x => x.ShippingStreet).HasMaxLength(250);
            entity.Property(x => x.ShippingBuildingNumber).HasMaxLength(50);
            entity.Property(x => x.ShippingApartment).HasMaxLength(100);
            entity.Property(x => x.ShippingInstructions).HasMaxLength(1000);
            entity.HasOne(x => x.ShippingGovernorate)
                .WithMany(x => x.Customers)
                .HasForeignKey(x => x.ShippingGovernorateId)
                .OnDelete(DeleteBehavior.Restrict);
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
            entity.HasIndex(x => x.NameAr).IsUnique();
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
            entity.HasIndex(x => x.NameAr).IsUnique();
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
            entity.Property(x => x.Sku).HasMaxLength(64).IsRequired();
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
            entity.HasIndex(x => x.Sku).IsUnique();
        });

        modelBuilder.Entity<Cart>(entity =>
        {
            entity.ToTable("Carts");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.UserId).HasMaxLength(450).IsRequired();
            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => x.UserId).IsUnique();
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.ToTable("CartItems", table =>
                table.HasCheckConstraint("CK_CartItems_Quantity_Positive", "[Quantity] > 0"));
            entity.HasKey(x => x.Id);
            entity.HasOne(x => x.Cart)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.CartId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.CartId, x.ProductId }).IsUnique();
        });

        modelBuilder.Entity<Governorate>(entity =>
        {
            entity.ToTable("Governorates", table =>
                table.HasCheckConstraint("CK_Governorates_ShippingFee_NonNegative", "[ShippingFee] >= 0"));
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.ShippingFee).HasPrecision(18, 2).IsRequired();
            entity.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("Orders", table =>
            {
                table.HasCheckConstraint("CK_Orders_Money_NonNegative", "[Subtotal] >= 0 AND [ShippingFee] >= 0 AND [Total] >= 0");
                table.HasCheckConstraint("CK_Orders_Status", "[Status] IN ('Pending', 'Confirmed', 'Processing', 'Shipped', 'Delivered', 'Cancelled')");
                table.HasCheckConstraint("CK_Orders_PaymentMethod", "[PaymentMethod] = 'COD'");
                table.HasCheckConstraint("CK_Orders_PaymentStatus", "[PaymentStatus] IN ('Pending', 'Paid', 'Failed')");
            });
            entity.HasKey(x => x.Id);
            entity.Property(x => x.OrderNumber).HasMaxLength(32).IsRequired();
            entity.Property(x => x.UserId).HasMaxLength(450).IsRequired();
            entity.Property(x => x.ClientRequestId).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(20).IsRequired();
            entity.Property(x => x.RowVersion).IsRowVersion();
            entity.Property(x => x.PaymentMethod).HasMaxLength(20).IsRequired();
            entity.Property(x => x.PaymentStatus).HasMaxLength(20).IsRequired();
            entity.Property(x => x.PaidAt);
            entity.Property(x => x.Subtotal).HasPrecision(18, 2);
            entity.Property(x => x.ShippingFee).HasPrecision(18, 2);
            entity.Property(x => x.Total).HasPrecision(18, 2);
            entity.Property(x => x.ShippingFullName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.ShippingPhone).HasMaxLength(30).IsRequired();
            entity.Property(x => x.ShippingGovernorateName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.ShippingCity).HasMaxLength(150).IsRequired();
            entity.Property(x => x.ShippingDistrict).HasMaxLength(150).IsRequired();
            entity.Property(x => x.ShippingStreet).HasMaxLength(250).IsRequired();
            entity.Property(x => x.ShippingBuildingNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.ShippingApartment).HasMaxLength(100);
            entity.Property(x => x.ShippingInstructions).HasMaxLength(1000);
            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.OrderNumber).IsUnique();
            entity.HasIndex(x => new { x.UserId, x.ClientRequestId }).IsUnique();
            entity.HasIndex(x => new { x.UserId, x.CreatedAt });
            entity.HasIndex(x => x.CreatedAt);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("OrderItems", table =>
            {
                table.HasCheckConstraint("CK_OrderItems_Quantity_Positive", "[Quantity] > 0");
                table.HasCheckConstraint("CK_OrderItems_Money_NonNegative", "[UnitPriceSnapshot] >= 0 AND [LineTotal] >= 0");
            });
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ProductNameSnapshot).HasMaxLength(250).IsRequired();
            entity.Property(x => x.SkuSnapshot).HasMaxLength(64).IsRequired();
            entity.Property(x => x.UnitPriceSnapshot).HasPrecision(18, 2);
            entity.Property(x => x.LineTotal).HasPrecision(18, 2);
            entity.HasOne(x => x.Order)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.OrderId);
        });

        modelBuilder.Entity<OrderStatusHistory>(entity =>
        {
            entity.ToTable("OrderStatusHistory");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Status).HasMaxLength(20).IsRequired();
            entity.Property(x => x.ChangedByUserId).HasMaxLength(450);
            entity.HasOne(x => x.Order)
                .WithMany(x => x.StatusHistory)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.OrderId, x.ChangedAt });
        });
    }
}
