using Microsoft.EntityFrameworkCore;
using TransactionService.Data;
using TransactionService.Services;

namespace TransactionService
{
    public class TransactionService : ITransactionService
    {
        private readonly TransactionDbContext _context;

        public TransactionService(TransactionDbContext context)
        {
            _context = context;
        }

        public async Task<bool> UnblockTransactionAsync(Guid transactionId)
        {
            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.Id == transactionId);

            if (transaction == null || !transaction.IsBlocked)
                return false;

            transaction.IsBlocked = false;
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
