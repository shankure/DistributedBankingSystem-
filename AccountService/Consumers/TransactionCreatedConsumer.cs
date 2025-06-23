using MassTransit;
using Shared.Messages;
using AccountService.Services;

namespace AccountService.Consumers;

public class TransactionCreatedConsumer : IConsumer<TransactionCreatedEvent>
{
    private readonly IAccountService _accountService;

    public TransactionCreatedConsumer(IAccountService accountService)
    {
        _accountService = accountService;
    }

    public async Task Consume(ConsumeContext<TransactionCreatedEvent> context)
    {
        var msg = context.Message;

        Console.WriteLine($"[Consumer] Handling transaction {msg.TransactionId}");

        if (!string.IsNullOrEmpty(msg.SenderAccountId))
        {
            await _accountService.UpdateBalanceAsync(Guid.Parse(msg.SenderAccountId), -msg.Amount);
        }

        if (!string.IsNullOrEmpty(msg.ReceiverAccountId))
        {
            await _accountService.UpdateBalanceAsync(Guid.Parse(msg.ReceiverAccountId), msg.Amount);
        }
    }
}
