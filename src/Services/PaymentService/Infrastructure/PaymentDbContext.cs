using System;
using Microsoft.EntityFrameworkCore;
using PaymentService.Domain;
using PaymentService.Infrastructure.Outbox;

namespace PaymentService.Infrastructure;

public class PaymentDbContext : DbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options) { }

    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Payment
        var p = modelBuilder.Entity<Payment>();
        p.HasKey(x => x.PaymentId);
        p.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        p.Property(x => x.Currency).HasMaxLength(8);
        p.Property(x => x.Method).HasMaxLength(32);
        p.Property(x => x.Status).HasConversion<int>();
        p.HasIndex(x => x.MerchantId);
        base.OnModelCreating(modelBuilder);

        // Outbox
        var o = modelBuilder.Entity<OutboxMessage>();
        o.HasKey(x => x.Id);
        o.Property(x => x.Type).HasMaxLength(512);
        o.Property(x => x.Payload);
        o.HasIndex(x => x.ProcessedOnUtc);
        base.OnModelCreating(modelBuilder);
    }
}
