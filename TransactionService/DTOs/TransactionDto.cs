namespace TransactionService.Dtos
{
    public class TransactionDto
    {
        public Guid Id { get; set; }
        public Guid FromAccountId { get; set; }
        public Guid? ToAccountId { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; } = "";
        public DateTime Timestamp { get; set; }
    }
}
