using Microsoft.EntityFrameworkCore;
using TransactionIngestion.Data;
using TransactionIngestion.Models;

namespace TransactionIngestion.Services;

public class TransactionReconciliationService
{
    private readonly AppDbContext _db;

    public TransactionReconciliationService(AppDbContext db)
    {
        _db = db;
    }

    public async Task ProcessSnapshotAsync(List<TransactionSnapshotDto> snapshot)
    {
        foreach (var dto in snapshot)
        {
            var existing = await _db.Transactions
                .FirstOrDefaultAsync(t => t.TransactionId == dto.transactionId);

            if (existing == null)
            {
                var transaction = new Transaction
                {
                    TransactionId = dto.transactionId,
                    CardLast4 = dto.cardNumber[^4..],
                    LocationCode = dto.locationCode,
                    ProductName = dto.productName,
                    Amount = dto.amount,
                    TransactionTime = dto.timestamp,
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _db.Transactions.Add(transaction);
                continue;
            }

            await DetectChanges(existing, dto);
        }

        await _db.SaveChangesAsync();
    }

    private async Task DetectChanges(Transaction existing, TransactionSnapshotDto dto)
    {
        CheckChange(existing, "LocationCode", existing.LocationCode, dto.locationCode);
        CheckChange(existing, "ProductName", existing.ProductName, dto.productName);
        CheckChange(existing, "Amount", existing.Amount.ToString(), dto.amount.ToString());

        var newLast4 = dto.cardNumber[^4..];
        CheckChange(existing, "CardLast4", existing.CardLast4, newLast4);

        existing.LocationCode = dto.locationCode;
        existing.ProductName = dto.productName;
        existing.Amount = dto.amount;
        existing.CardLast4 = newLast4;

        existing.UpdatedAt = DateTime.UtcNow;

        await Task.CompletedTask;
    }

    private void CheckChange(Transaction transaction, string field, string oldValue, string newValue)
    {
        if (oldValue == newValue)
            return;

        var audit = new TransactionAudit
        {
            TransactionId = transaction.TransactionId,
            FieldName = field,
            OldValue = oldValue,
            NewValue = newValue,
            ChangedAt = DateTime.UtcNow
        };

        _db.TransactionAudits.Add(audit);
    }
}