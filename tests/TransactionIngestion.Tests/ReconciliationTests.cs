using Microsoft.EntityFrameworkCore;
using TransactionIngestion.Data;
using TransactionIngestion.Models;
using TransactionIngestion.Services;

public class ReconciliationTests
{
    private AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task Inserts_New_Transaction()
    {
        var db = CreateDbContext();
        var service = new TransactionReconciliationService(db);

        var snapshot = new List<TransactionSnapshotDto>
        {
            new()
            {
                transactionId = "T-1",
                cardNumber = "4111111111111111",
                locationCode = "STO-1",
                productName = "Mouse",
                amount = 20,
                timestamp = DateTime.UtcNow
            }
        };

        await service.ProcessSnapshotAsync(snapshot);

        Assert.Single(db.Transactions);
    }

    [Fact]
    public async Task Detects_Transaction_Update()
    {
        var db = CreateDbContext();

        db.Transactions.Add(new Transaction
        {
            TransactionId = "T-1",
            CardLast4 = "1111",
            LocationCode = "STO-1",
            ProductName = "Mouse",
            Amount = 20,
            TransactionTime = DateTime.UtcNow,
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        var service = new TransactionReconciliationService(db);

        var snapshot = new List<TransactionSnapshotDto>
        {
            new()
            {
                transactionId = "T-1",
                cardNumber = "4111111111111111",
                locationCode = "STO-1",
                productName = "Keyboard",
                amount = 20,
                timestamp = DateTime.UtcNow
            }
        };

        await service.ProcessSnapshotAsync(snapshot);

        Assert.Single(db.TransactionAudits);
    }

    [Fact]
    public async Task Revokes_Missing_Transaction()
    {
        var db = CreateDbContext();

        db.Transactions.Add(new Transaction
        {
            TransactionId = "T-1",
            CardLast4 = "1111",
            LocationCode = "STO-1",
            ProductName = "Mouse",
            Amount = 20,
            TransactionTime = DateTime.UtcNow,
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        var service = new TransactionReconciliationService(db);

        await service.ProcessSnapshotAsync(new List<TransactionSnapshotDto>());

        var transaction = db.Transactions.First();

        Assert.Equal("Revoked", transaction.Status);
    }
}