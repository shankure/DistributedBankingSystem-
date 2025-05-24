using LedgerService.Models;
using Microsoft.EntityFrameworkCore;

namespace LedgerService.Data
{
    public class LedgerDbContext : DbContext
    {
        public LedgerDbContext(DbContextOptions<LedgerDbContext> options) : base(options) { }

        public DbSet<LedgerEntry> LedgerEntries => Set<LedgerEntry>();
    }
}
