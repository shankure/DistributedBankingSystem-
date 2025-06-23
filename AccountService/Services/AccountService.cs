using AccountService.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace AccountService.Services
{
    public class AccountService : IAccountService
    {
        private readonly AccountDbContext _context;

        public AccountService(AccountDbContext context)
        {
            _context = context;
        }

        public async Task UpdateBalanceAsync(Guid accountId, decimal amount)
        {
            var account = await _context.BankAccounts.FirstOrDefaultAsync(a => a.Id == accountId);

            if (account == null)
            {
                Console.WriteLine($"[AccountService] Account {accountId} not found.");
                return;
            }

            account.Balance += amount;
            await _context.SaveChangesAsync();

            Console.WriteLine($"[AccountService] Updated balance for Account {accountId}: {account.Balance}");
        }
    }
}
