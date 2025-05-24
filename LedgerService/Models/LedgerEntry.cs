namespace LedgerService.Models
{
    public class LedgerEntry
    {
        public Guid Id { get; set; }
        public Guid TransactionId { get; set; }
        public Guid? FromAccountId { get; set; }
        public Guid? ToAccountId { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; } = ""; // Deposit, Withdrawal, Transfer
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "COMPLETED"; // or PENDING, FAILED
        public string Notes { get; set; } = "";
    }
}
