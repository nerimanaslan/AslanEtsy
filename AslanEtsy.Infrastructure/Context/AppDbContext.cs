using AslanEtsy.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AslanEtsy.Infrastructure.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<EtsyAccount> EtsyAccounts => Set<EtsyAccount>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderTracking> OrderTrackings => Set<OrderTracking>();
    public DbSet<SyncLog> SyncLogs => Set<SyncLog>();
    public DbSet<CurtainProduct> CurtainProducts => Set<CurtainProduct>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Global Query Filter for soft delete
        modelBuilder.Entity<EtsyAccount>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Order>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<OrderItem>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<OrderTracking>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<SyncLog>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<CurtainProduct>().HasQueryFilter(e => !e.IsDeleted);

        // EtsyAccount config
        modelBuilder.Entity<EtsyAccount>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ShopId);
            entity.HasIndex(e => e.OAuthState);
            entity.Property(e => e.ShopName).IsRequired().HasMaxLength(250);
            entity.Property(e => e.Keystring).IsRequired().HasMaxLength(250);
        });

        // Order config
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ReceiptId).IsUnique();
            entity.HasIndex(e => e.EtsyAccountId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CustomStatus);
            entity.HasIndex(e => e.OrderDateUtc);

            entity.Property(e => e.BuyerName).HasMaxLength(250);
            entity.Property(e => e.BuyerEmail).HasMaxLength(250);
            entity.Property(e => e.CurrencyCode).HasMaxLength(10);
            entity.Property(e => e.GrandTotalAmount).HasPrecision(18, 4);
            entity.Property(e => e.SubtotalAmount).HasPrecision(18, 4);
            entity.Property(e => e.ShippingAmount).HasPrecision(18, 4);
            entity.Property(e => e.TaxAmount).HasPrecision(18, 4);
            entity.Property(e => e.DiscountAmount).HasPrecision(18, 4);

            entity.HasOne(e => e.EtsyAccount)
                  .WithMany(a => a.Orders)
                  .HasForeignKey(e => e.EtsyAccountId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // OrderItem config
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.OrderId);
            entity.HasIndex(e => e.TransactionId);
            entity.HasIndex(e => e.ListingId);

            entity.Property(e => e.Title).HasMaxLength(500);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 4);
            entity.Property(e => e.CurrencyCode).HasMaxLength(10);

            entity.HasOne(e => e.Order)
                  .WithMany(o => o.Items)
                  .HasForeignKey(e => e.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // OrderTracking config
        modelBuilder.Entity<OrderTracking>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.OrderId);
            entity.Property(e => e.TrackingCode).HasMaxLength(150);
            entity.Property(e => e.CarrierName).HasMaxLength(100);

            entity.HasOne(e => e.Order)
                  .WithMany(o => o.Trackings)
                  .HasForeignKey(e => e.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // SyncLog config
        modelBuilder.Entity<SyncLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.EtsyAccountId);
            entity.HasIndex(e => e.StartedAtUtc);

            entity.HasOne(e => e.EtsyAccount)
                  .WithMany(a => a.SyncLogs)
                  .HasForeignKey(e => e.EtsyAccountId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
