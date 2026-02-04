using Beyond8.Common.Data.Base;
using Beyond8.Sale.Domain.Entities;
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);
            entity.HasIndex(e => e.OrderNumber).IsUnique();
            entity.HasIndex(e => e.UserId);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);
        });

        modelBuilder.Entity<Coupon>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);
            entity.HasIndex(e => e.Code).IsUnique();
        });

        modelBuilder.Entity<CouponUsage>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);
        });

        modelBuilder.Entity<InstructorWallet>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);
            entity.HasIndex(e => e.InstructorId).IsUnique();
        });

        modelBuilder.Entity<PayoutRequest>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);
        });

        modelBuilder.Entity<TransactionLedger>(entity =>
        {
            entity.HasQueryFilter(e => e.DeletedAt == null);
        });
    }
}
