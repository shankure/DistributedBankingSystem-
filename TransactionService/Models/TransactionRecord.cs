namespace TransactionService.Models
{
    public class TransactionRecord
    {
        public Guid Id { get; set; }
        public Guid FromAccountId { get; set; }
        public Guid? ToAccountId { get; set; } // null for withdrawals
        public decimal Amount { get; set; }
        public string Type { get; set; } = ""; // Deposit, Withdrawal, Transfer
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
