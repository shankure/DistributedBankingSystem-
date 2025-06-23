using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Messages;
using Shared.Messages.Models;
using System.Text.Json;

namespace FraudDetectionService.Consumers
{
    public class FraudDetectionConsumer : IConsumer<TransactionInitiatedEvent>
    {
        private readonly ILogger<FraudDetectionConsumer> _logger;

        public FraudDetectionConsumer(ILogger<FraudDetectionConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<TransactionInitiatedEvent> context)
        {
            var transactionEvent = context.Message;

            _logger.LogInformation("🕵️‍♂️ FRAUD CHECK START – TX ID: {TransactionId}", transactionEvent.TransactionId);
            _logger.LogInformation("Amount: ${Amount}", transactionEvent.Amount);
            _logger.LogInformation("Timestamp: {Timestamp}", transactionEvent.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"));

            // Simple fraud rules
            bool isHighAmount = transactionEvent.Amount > 10_000;
            bool isOddHour = transactionEvent.Timestamp.Hour < 6;

            if (!isHighAmount && !isOddHour)
            {
                _logger.LogInformation("✅ NOT FRAUDULENT – Transaction passed all checks.");

                var cleanEvent = new TransactionCreatedEvent
                {
                    TransactionId = transactionEvent.TransactionId,
                    SenderAccountId = transactionEvent.SenderAccountId,
                    ReceiverAccountId = transactionEvent.ReceiverAccountId,
                    Amount = transactionEvent.Amount,
                    Timestamp = transactionEvent.Timestamp
                };

                await context.Publish(cleanEvent);
                _logger.LogInformation("📤 Clean Event Published – TX ID: {TransactionId}", transactionEvent.TransactionId);
                return;
            }

            var reason = isHighAmount ? "High-value transaction" : "Unusual transaction time";

            var fraudAlert = new FraudAlertMessage
            {
                TransactionId = transactionEvent.TransactionId,
                SenderEmail = "noreply@bankingsystem.com",
                ReceiverEmail = transactionEvent.ReceiverEmail, // replace with your Mailtrap address if needed
                Amount = transactionEvent.Amount,
                Timestamp = transactionEvent.Timestamp,
                Reason = reason
            };

            var serialized = JsonSerializer.Serialize(fraudAlert, new JsonSerializerOptions { WriteIndented = true });
            _logger.LogWarning("🚨 FRAUD DETECTED – Reason: {Reason}", reason);
            _logger.LogInformation("📦 FraudAlertMessage JSON:\n{Json}", serialized);

            await context.Publish(fraudAlert);
            _logger.LogInformation("📤 Fraud Alert Published – TX ID: {TransactionId}", transactionEvent.TransactionId);

            _logger.LogInformation("✔️ Completed Fraud Check – TX {TransactionId}", transactionEvent.TransactionId);
        }
    }
}