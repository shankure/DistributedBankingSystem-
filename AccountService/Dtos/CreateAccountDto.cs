namespace AccountService.Dtos
{
    public class CreateAccountDto
    {
        public string AccountType { get; set; } = "Checking";
        public decimal InitialDeposit { get; set; } = 0;
    }
}
