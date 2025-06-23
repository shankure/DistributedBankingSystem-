using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Messages;
using LedgerService.Data;
using LedgerService.Models;
using LedgerService.Dtos;
using System.Net.Http;
using System.Net.Http.Json;

namespace LedgerService.Consumers
{
    public class TransactionCreatedConsumer : IConsumer<TransactionCreatedEvent>
    {
        private readonly LedgerDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<TransactionCreatedConsumer> _logger;

        public TransactionCreatedConsumer(LedgerDbContext context, IHttpClientFactory httpClientFactory, ILogger<TransactionCreatedConsumer> logger)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<TransactionCreatedEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation("📥 Received TransactionCreatedEvent: {TransactionId}", message.TransactionId);

            var ledgerEntry = new LedgerEntry
            {
                Id = Guid.NewGuid(),
                TransactionId = message.TransactionId,
                FromAccountId = Guid.Parse(message.SenderAccountId),
                ToAccountId = string.IsNullOrEmpty(message.ReceiverAccountId) ? null : Guid.Parse(message.ReceiverAccountId),
                Amount = message.Amount,
                Type = string.IsNullOrEmpty(message.ReceiverAccountId) ? "Withdrawal" : "Transfer",
                Timestamp = message.Timestamp,
                Status = "COMPLETED",
                Notes = "Recorded by LedgerService"
            };

            _context.LedgerEntries.Add(ledgerEntry);
            await _context.SaveChangesAsync();
            _logger.LogInformation("✅ Ledger entry saved for transaction {TransactionId}", message.TransactionId);

            try
            {
                var client = _httpClientFactory.CreateClient("AccountService");
                _logger.LogInformation("🌐 Sending HTTP request to AccountService: {BaseUrl}", client.BaseAddress);

                var subtractDto = new BalanceUpdateDto
                {
                    AccountId = Guid.Parse(message.SenderAccountId),
                    Amount = -message.Amount
                };

                _logger.LogInformation("📤 Subtracting {Amount} from sender account {AccountId}", subtractDto.Amount, subtractDto.AccountId);
                var subtractRes = await client.PostAsJsonAsync("/api/bankaccounts/update-balance", subtractDto);
                _logger.LogInformation("📥 Response from sender balance update: {StatusCode}", subtractRes.StatusCode);
                var subtractContent = await subtractRes.Content.ReadAsStringAsync();
                _logger.LogInformation("📥 Response content: {Content}", subtractContent);
                subtractRes.EnsureSuccessStatusCode();
                _logger.LogInformation("💸 Deducted from sender {Sender}", message.SenderAccountId);

                if (!string.IsNullOrEmpty(message.ReceiverAccountId))
                {
                    var addDto = new BalanceUpdateDto
                    {
                        AccountId = Guid.Parse(message.ReceiverAccountId),
                        Amount = message.Amount
                    };

                    _logger.LogInformation("📤 Adding {Amount} to receiver account {AccountId}", addDto.Amount, addDto.AccountId);
                    var addRes = await client.PostAsJsonAsync("/api/bankaccounts/update-balance", addDto);
                    _logger.LogInformation("📥 Response from receiver balance update: {StatusCode}", addRes.StatusCode);
                    var addContent = await addRes.Content.ReadAsStringAsync();
                    _logger.LogInformation("📥 Response content: {Content}", addContent);
                    addRes.EnsureSuccessStatusCode();
                    _logger.LogInformation("💰 Credited to receiver {Receiver}", message.ReceiverAccountId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to update balances for transaction {TransactionId}", message.TransactionId);
            }
        }
    }
}