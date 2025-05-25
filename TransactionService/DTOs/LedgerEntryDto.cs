namespace TransactionService.Dtos
{
    public class LedgerEntryDto
    {
        public Guid TransactionId { get; set; }
        public Guid? FromAccountId { get; set; }
        public Guid? ToAccountId { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; } = "";
        public string Status { get; set; } = "COMPLETED";
        public string Notes { get; set; } = "";
    }
}
