namespace TransactionService.Dtos
{
    public class CreateTransactionDto
    {
        public Guid FromAccountId { get; set; }
        public Guid? ToAccountId { get; set; } // Null for withdrawal
        public decimal Amount { get; set; }
        public string Type { get; set; } = ""; // Deposit, Withdrawal, Transfer
    }
}
