namespace AccountService.Models
{
    public class BankAccount
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }  // Foreign key reference (not enforced yet)
        public string AccountType { get; set; } = "Checking"; // or Savings
        public decimal Balance { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
