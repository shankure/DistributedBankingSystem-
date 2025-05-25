namespace LedgerService.Dtos
{
    public class BalanceUpdateDto
    {
        public Guid AccountId { get; set; }
        public decimal Amount { get; set; }
    }
}
