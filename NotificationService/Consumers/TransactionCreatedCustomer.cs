using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationService.Services;
using Shared.Messages;
using System.Threading.Tasks;

namespace NotificationService.Consumers
{
    public class TransactionCreatedConsumer : IConsumer<TransactionCreatedEvent>
    {
        private readonly EmailSender _emailSender;
        private readonly ILogger<TransactionCreatedConsumer> _logger;

        public TransactionCreatedConsumer(EmailSender emailSender, ILogger<TransactionCreatedConsumer> logger)
        {
            _emailSender = emailSender;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<TransactionCreatedEvent> context)
        {
            var transaction = context.Message;

            if (transaction == null || string.IsNullOrWhiteSpace(transaction.SenderAccountId))
            {
                _logger.LogWarning("⚠️ Skipping empty or invalid TransactionCreatedEvent.");
                return;
            }

            _logger.LogInformation("📩 Received TransactionCreatedEvent for {TransactionId}", transaction.TransactionId);

            var subject = $"💸 You Sent ${transaction.Amount}";

            var bodyText = $"""
            You successfully sent ${transaction.Amount} to {transaction.ReceiverAccountId}
            on {transaction.Timestamp:yyyy-MM-dd HH:mm:ss}.

            Transaction ID: {transaction.TransactionId}
            """;

            var htmlBody = $"""
            <h3>💸 You Sent <strong>${transaction.Amount}</strong></h3>
            <ul>
                <li><strong>To:</strong> {transaction.ReceiverAccountId}</li>
                <li><strong>When:</strong> {transaction.Timestamp:yyyy-MM-dd HH:mm:ss}</li>
                <li><strong>Transaction ID:</strong> {transaction.TransactionId}</li>
            </ul>
            """;

            await _emailSender.SendEmailAsync(transaction.SenderAccountId, subject, bodyText, htmlBody);

            _logger.LogInformation("📤 Transaction email sent to {SenderEmail}", transaction.SenderAccountId);
        }
    }
}