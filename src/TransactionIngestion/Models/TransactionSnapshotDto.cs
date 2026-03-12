namespace TransactionIngestion.Models;

public class TransactionSnapshotDto
{
    public string transactionId { get; set; } = "";
    public string cardNumber { get; set; } = "";
    public string locationCode { get; set; } = "";
    public string productName { get; set; } = "";
    public decimal amount { get; set; }
    public DateTime timestamp { get; set; }
}