using System;
using Microsoft.EntityFrameworkCore;
using PaymentService.Domain;

namespace PaymentService.Infrastructure;

public class PaymentDbContext : DbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options) { }

    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var p = modelBuilder.Entity<Payment>();
        p.HasKey(x => x.PaymentId);
        p.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        p.Property(x => x.Currency).HasMaxLength(8);
        p.Property(x => x.Method).HasMaxLength(32);
        p.Property(x => x.Status).HasConversion<int>();
        p.HasIndex(x => x.MerchantId);
        base.OnModelCreating(modelBuilder);
    }
}
