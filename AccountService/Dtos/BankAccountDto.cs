namespace AccountService.Dtos
{
    public class BankAccountDto
    {
        public Guid Id { get; set; }
        public string AccountType { get; set; }
        public decimal Balance { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
