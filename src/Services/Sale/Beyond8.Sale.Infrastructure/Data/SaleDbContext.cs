using Beyond8.Common.Data.Base;
using Beyond8.Sale.Domain.Entities;
using Beyond8.Sale.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Beyond8.Sale.Infrastructure.Data;

public class SaleDbContext : BaseDbContext
{
    public SaleDbContext(DbContextOptions<SaleDbContext> options) : base(options)
    {
    }

    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<OrderItem> OrderItems { get; set; } = null!;
    public DbSet<Payment> Payments { get; set; } = null!;
    public DbSet<Coupon> Coupons { get; set; } = null!;
    public DbSet<CouponUsage> CouponUsages { get; set; } = null!;
    public DbSet<InstructorWallet> InstructorWallets { get; set; } = null!;
    public DbSet<PayoutRequest> PayoutRequests { get; set; } = null!;
    public DbSet<TransactionLedger> TransactionLedgers { get; set; } = null!;
    public DbSet<Cart> Carts { get; set; } = null!;
    public DbSet<CartItem> CartItems { get; set; } = null!;
    public DbSet<PlatformWallet> PlatformWallets { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Order Configuration
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);

            // Unique Constraints
            entity.HasIndex(e => e.OrderNumber).IsUnique();

            // Performance Indexes
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.PaidAt);
            entity.HasIndex(e => new { e.Status, e.PaidAt });

            // JSONB Column
            entity.Property(e => e.PaymentDetails)
                .HasColumnType("jsonb");

            // Relationships
            entity.HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Payment relationship configured in Payment entity config (supports nullable OrderId)

            entity.HasOne(o => o.Coupon)
                .WithMany()
                .HasForeignKey(o => o.CouponId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // OrderItem Configuration
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);

            entity.HasIndex(e => e.OrderId);
            entity.HasIndex(e => e.CourseId);
            entity.HasIndex(e => e.InstructorId);
        });

        // Payment Configuration
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);

            // Unique Constraints
            entity.HasIndex(e => e.PaymentNumber).IsUnique();

            // Performance Indexes
            entity.HasIndex(e => e.OrderId)
                .HasFilter("\"OrderId\" IS NOT NULL");
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => new { e.Provider, e.Status });
            entity.HasIndex(e => e.ExternalTransactionId);
            entity.HasIndex(e => e.PaidAt);
            entity.HasIndex(e => e.Purpose);
            entity.HasIndex(e => e.WalletId)
                .HasFilter("\"WalletId\" IS NOT NULL");

            // Default values
            entity.Property(e => e.Purpose)
                .HasDefaultValue(PaymentPurpose.OrderPayment);

            // JSONB Column
            entity.Property(e => e.Metadata)
                .HasColumnType("jsonb");

            // Relationships - Order is optional (null for WalletTopUp)
            entity.HasOne(p => p.Order)
                .WithMany(o => o.Payments)
                .HasForeignKey(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);

            // Relationship - InstructorWallet for TopUp payments
            entity.HasOne(p => p.InstructorWallet)
                .WithMany()
                .HasForeignKey(p => p.WalletId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);
        });

        // Coupon Configuration
        modelBuilder.Entity<Coupon>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);

            // Unique Constraints
            entity.HasIndex(e => e.Code).IsUnique();

            // Performance Indexes
            entity.HasIndex(e => new { e.IsActive, e.ValidFrom, e.ValidTo });
            entity.HasIndex(e => e.ApplicableInstructorId)
                .HasFilter("\"ApplicableInstructorId\" IS NOT NULL");
            entity.HasIndex(e => e.ApplicableCourseId)
                .HasFilter("\"ApplicableCourseId\" IS NOT NULL");

            // Default values for hold amounts
            entity.Property(e => e.HoldAmount)
                .HasDefaultValue(0m);
            entity.Property(e => e.RemainingHoldAmount)
                .HasDefaultValue(0m);

            // Relationships
            entity.HasMany(c => c.CouponUsages)
                .WithOne(cu => cu.Coupon)
                .HasForeignKey(cu => cu.CouponId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // CouponUsage Configuration
        modelBuilder.Entity<CouponUsage>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);

            entity.HasIndex(e => e.CouponId);
            entity.HasIndex(e => e.OrderId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => new { e.CouponId, e.UserId });

            // Relationships
            entity.HasOne(cu => cu.Order)
                .WithMany()
                .HasForeignKey(cu => cu.OrderId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // InstructorWallet Configuration
        modelBuilder.Entity<InstructorWallet>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);

            // Unique Constraints
            entity.HasIndex(e => e.InstructorId).IsUnique();

            // Performance Indexes
            entity.HasIndex(e => new { e.IsActive, e.AvailableBalance });

            // Default values for HoldBalance
            entity.Property(e => e.HoldBalance)
                .HasDefaultValue(0m);

            // JSONB Column
            entity.Property(e => e.BankAccountInfo)
                .HasColumnType("jsonb");

            // Relationships
            entity.HasMany(w => w.Transactions)
                .WithOne(t => t.InstructorWallet)
                .HasForeignKey(t => t.WalletId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(w => w.PayoutRequests)
                .WithOne(p => p.InstructorWallet)
                .HasForeignKey(p => p.WalletId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // PayoutRequest Configuration
        modelBuilder.Entity<PayoutRequest>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);

            // Unique Constraints
            entity.HasIndex(e => e.RequestNumber).IsUnique();

            // Performance Indexes
            entity.HasIndex(e => e.WalletId);
            entity.HasIndex(e => e.InstructorId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.RequestedAt);
            entity.HasIndex(e => new { e.Status, e.RequestedAt });
        });

        // TransactionLedger Configuration
        modelBuilder.Entity<TransactionLedger>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);

            // Performance Indexes
            entity.HasIndex(e => new { e.WalletId, e.CreatedAt });
            entity.HasIndex(e => new { e.ReferenceId, e.ReferenceType });
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.Status);

            // JSONB Column
            entity.Property(e => e.Metadata)
                .HasColumnType("jsonb");
        });

        // Cart Configuration
        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);

            // One cart per user
            entity.HasIndex(e => e.UserId).IsUnique();

            // Relationships
            entity.HasMany(c => c.CartItems)
                .WithOne(ci => ci.Cart)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // CartItem Configuration
        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);

            entity.HasIndex(e => e.CartId);
            entity.HasIndex(e => e.CourseId);

            // Prevent duplicate courses in one cart
            entity.HasIndex(e => new { e.CartId, e.CourseId }).IsUnique();
        });

        // PlatformWallet Configuration
        modelBuilder.Entity<PlatformWallet>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);

            // Default values
            entity.Property(e => e.AvailableBalance).HasDefaultValue(0m);
            entity.Property(e => e.TotalRevenue).HasDefaultValue(0m);
            entity.Property(e => e.TotalCouponCost).HasDefaultValue(0m);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });
    }
}
