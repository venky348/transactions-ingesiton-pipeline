using Microsoft.EntityFrameworkCore;
using TransactionIngestion.Models;

namespace TransactionIngestion.Data;

public class AppDbContext : DbContext
{
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<TransactionAudit> TransactionAudits => Set<TransactionAudit>();

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Transaction>()
            .HasIndex(t => t.TransactionId)
            .IsUnique();

        base.OnModelCreating(modelBuilder);
    }
}