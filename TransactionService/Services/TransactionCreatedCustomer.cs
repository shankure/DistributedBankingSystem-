using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Messages;
using TransactionService.Data;
using TransactionAbstractions.Models;

namespace TransactionService.Services
{
    public class TransactionCreatedConsumer : IConsumer<TransactionCreatedEvent>
    {
        private readonly TransactionDbContext _context;

        public TransactionCreatedConsumer(TransactionDbContext context)
        {
            _context = context;
        }

        public async Task Consume(ConsumeContext<TransactionCreatedEvent> context)
        {
            var msg = context.Message;
            Console.WriteLine($"[Consumer] Received TransactionCreatedEvent: {msg.TransactionId}");

            var exists = await _context.Transactions.AnyAsync(t => t.Id == msg.TransactionId);
            if (exists)
            {
                Console.WriteLine($"[Consumer] Transaction {msg.TransactionId} already exists. Skipping.");
                return;
            }

            var transaction = new TransactionRecord
            {
                Id = msg.TransactionId,
                FromAccountId = Guid.Parse(msg.SenderAccountId),
                ToAccountId = Guid.Parse(msg.ReceiverAccountId),
                Amount = msg.Amount,
                Timestamp = msg.Timestamp,
                Type = "Transfer", // default or optional
                IsBlocked = false   // optional pre-block for fraud alerts
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            Console.WriteLine($"[Consumer] SAVED missing transaction {transaction.Id} from event.");
        }
    }
}
