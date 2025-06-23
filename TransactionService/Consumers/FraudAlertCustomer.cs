using MassTransit;
using Shared.Messages.Models; // Or wherever your FraudAlertMessage is
using TransactionService.Data;

namespace TransactionService.Consumers
{
    public class FraudAlertConsumer : IConsumer<FraudAlertMessage>
    {
        private readonly TransactionDbContext _context;

        public FraudAlertConsumer(TransactionDbContext context)
        {
            _context = context;
        }

        public async Task Consume(ConsumeContext<FraudAlertMessage> context)
        {
            var message = context.Message;
            var id = message.TransactionId;
            var tx = await _context.Transactions.FindAsync(id);

            if (tx == null)
            {
                Console.WriteLine($"[FraudAlertConsumer] Transaction {id} not found.");
                return;
            }

            tx.IsBlocked = true;
            await _context.SaveChangesAsync();

            Console.WriteLine($"🚨 [FraudAlertConsumer] Marked Transaction {id} as BLOCKED.");
        }
    }
}