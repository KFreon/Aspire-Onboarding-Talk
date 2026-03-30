using Microsoft.EntityFrameworkCore;
using Shared.Entities;

namespace App2.Data;

public class App2DbContext : DbContext
{
    public App2DbContext(DbContextOptions<App2DbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CustomerName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.CustomerEmail).HasMaxLength(200).IsRequired();
            entity.Property(e => e.TotalPrice).HasPrecision(18, 2);
            entity.Property(e => e.Status).HasMaxLength(50);
        });
    }
}
