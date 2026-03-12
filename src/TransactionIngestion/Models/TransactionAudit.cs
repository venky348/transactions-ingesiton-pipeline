using System;

namespace TransactionIngestion.Models;

public class TransactionAudit
{
    public int Id { get; set; }

    public string TransactionId { get; set; } = string.Empty;

    public string FieldName { get; set; } = string.Empty;

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public DateTime ChangedAt { get; set; }
}