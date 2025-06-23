using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using TransactionService.Data;
using Shared.Messages;
using TransactionAbstractions.Models;

namespace TransactionService.Controllers
{
    [ApiController]
    [Route("api/fraud")]
    public class FraudController : ControllerBase
    {
        private readonly TransactionDbContext _context;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<FraudController> _logger;

        public FraudController(TransactionDbContext context, IPublishEndpoint publishEndpoint, ILogger<FraudController> logger)
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        [HttpPost("unblock/{id}")]
        public async Task<IActionResult> UnblockTransaction(Guid id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
                return NotFound("❌ Transaction not found.");

            if (!transaction.IsBlocked)
                return BadRequest("✅ Transaction is already unblocked.");

            transaction.IsBlocked = false;
            await _context.SaveChangesAsync();
            _logger.LogInformation("✅ Transaction {TransactionId} unblocked", transaction.Id);

            await PublishTransactionEvent(transaction);
            return Ok("✅ Transaction has been unblocked and event published.");
        }

        [HttpGet("unblock/{id}")]
        public async Task<IActionResult> UnblockTransactionViaEmail(string id)
        {
            if (!Guid.TryParse(id, out Guid transactionId))
                return Content("❌ Invalid transaction ID.");

            var transaction = await _context.Transactions.FindAsync(transactionId);
            if (transaction == null)
                return Content("❌ Transaction not found.");

            if (!transaction.IsBlocked)
                return Content("ℹ️ Transaction was already confirmed and balance was already updated.");

            transaction.IsBlocked = false;
            await _context.SaveChangesAsync();
            _logger.LogInformation("✅ Transaction {TransactionId} unblocked via email", transaction.Id);

            await PublishTransactionEvent(transaction);
            return Content("✅ Thank you! The transaction has been confirmed and unblocked.");
        }

        private async Task PublishTransactionEvent(TransactionRecord transaction)
        {
            try
            {
                var senderId = transaction.FromAccountId.ToString();
                var receiverId = transaction.ToAccountId?.ToString() ?? "";

                var @event = new TransactionCreatedEvent
                {
                    TransactionId = transaction.Id,
                    SenderAccountId = senderId,
                    ReceiverAccountId = receiverId,
                    Amount = transaction.Amount,
                    Timestamp = transaction.Timestamp
                };

                await _publishEndpoint.Publish(@event);
                _logger.LogInformation("📤 Published TransactionCreatedEvent for {TransactionId}", transaction.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to publish TransactionCreatedEvent for {TransactionId}", transaction.Id);
            }
        }
    }
}
