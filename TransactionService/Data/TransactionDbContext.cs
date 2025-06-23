using Microsoft.EntityFrameworkCore;
using TransactionAbstractions.Models;

namespace TransactionService.Data
{
    public class TransactionDbContext : DbContext
    {
        public TransactionDbContext(DbContextOptions<TransactionDbContext> options) : base(options) { }

        public DbSet<TransactionRecord> Transactions => Set<TransactionRecord>();
    }
}
