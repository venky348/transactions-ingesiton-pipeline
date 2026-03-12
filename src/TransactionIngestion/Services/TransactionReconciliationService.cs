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
        var snapshotIds = snapshot.Select(s => s.transactionId).ToHashSet();

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

        await HandleRevocations(snapshotIds);

        await HandleFinalization();

        await _db.SaveChangesAsync();
    }

    private async Task DetectChanges(Transaction existing, TransactionSnapshotDto dto)
    {
        var newLast4 = dto.cardNumber[^4..];

        CheckChange(existing, "LocationCode", existing.LocationCode, dto.locationCode);
        CheckChange(existing, "ProductName", existing.ProductName, dto.productName);
        CheckChange(existing, "Amount", existing.Amount.ToString(), dto.amount.ToString());
        CheckChange(existing, "CardLast4", existing.CardLast4, newLast4);

        existing.LocationCode = dto.locationCode;
        existing.ProductName = dto.productName;
        existing.Amount = dto.amount;
        existing.CardLast4 = newLast4;

        existing.UpdatedAt = DateTime.UtcNow;

        await Task.CompletedTask;
    }

    private void CheckChange(Transaction transaction, string fieldName, string oldValue, string newValue)
    {
        if (oldValue == newValue)
            return;

        var audit = new TransactionAudit
        {
            TransactionId = transaction.TransactionId,
            FieldName = fieldName,
            OldValue = oldValue,
            NewValue = newValue,
            ChangedAt = DateTime.UtcNow
        };

        _db.TransactionAudits.Add(audit);
    }

    private async Task HandleRevocations(HashSet<string> snapshotIds)
    {
        var cutoff = DateTime.UtcNow.AddHours(-24);

        var candidates = await _db.Transactions
            .Where(t =>
                t.TransactionTime >= cutoff &&
                !snapshotIds.Contains(t.TransactionId) &&
                t.Status != "Revoked")
            .ToListAsync();

        foreach (var transaction in candidates)
        {
            var audit = new TransactionAudit
            {
                TransactionId = transaction.TransactionId,
                FieldName = "Status",
                OldValue = transaction.Status,
                NewValue = "Revoked",
                ChangedAt = DateTime.UtcNow
            };

            transaction.Status = "Revoked";
            transaction.UpdatedAt = DateTime.UtcNow;

            _db.TransactionAudits.Add(audit);
        }
    }

    private async Task HandleFinalization()
    {
        var cutoff = DateTime.UtcNow.AddHours(-24);

        var candidates = await _db.Transactions
            .Where(t =>
                t.TransactionTime < cutoff &&
                t.Status != "Finalized")
            .ToListAsync();

        foreach (var transaction in candidates)
        {
            var audit = new TransactionAudit
            {
                TransactionId = transaction.TransactionId,
                FieldName = "Status",
                OldValue = transaction.Status,
                NewValue = "Finalized",
                ChangedAt = DateTime.UtcNow
            };

            transaction.Status = "Finalized";
            transaction.UpdatedAt = DateTime.UtcNow;

            _db.TransactionAudits.Add(audit);
        }
    }
}