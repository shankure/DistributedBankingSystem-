using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MassTransit;
using System.Security.Claims;
using TransactionService.Data;
using TransactionService.Dtos;
using TransactionAbstractions.Models;
using TransactionAbstractions.Dtos;
using TransactionService.Services;
using Shared.Messages.Models;
using Shared.Messages;
using Microsoft.Extensions.Logging;

namespace TransactionService.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly TransactionDbContext _context;
    private readonly ITransactionService _transactionService;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<TransactionsController> _logger;

    public TransactionsController(
        TransactionDbContext context,
        ITransactionService transactionService,
        IPublishEndpoint publishEndpoint,
        ILogger<TransactionsController> logger)
    {
        _context = context;
        _transactionService = transactionService;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> ProcessTransaction([FromBody] CreateTransactionDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized("Missing user ID in token.");

        var transaction = new TransactionRecord
        {
            Id = Guid.NewGuid(),
            FromAccountId = dto.FromAccountId,
            ToAccountId = dto.ToAccountId,
            Amount = dto.Amount,
            Type = dto.Type,
            Timestamp = DateTime.UtcNow,
            IsBlocked = true
        };

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        // ✅ Publish TransactionInitiatedEvent instead of TransactionCreatedEvent
        var initiatedEvent = new TransactionInitiatedEvent
        {
            TransactionId = transaction.Id,
            SenderAccountId = transaction.FromAccountId.ToString(),
            ReceiverAccountId = transaction.ToAccountId?.ToString(),
            Amount = transaction.Amount,
            Timestamp = transaction.Timestamp,
            SenderEmail = User.FindFirstValue(ClaimTypes.Email), // ✅ sender from JWT
            ReceiverEmail = "darko@example.com" // ✅ or any test email you want
        };

        await _publishEndpoint.Publish(initiatedEvent);
        _logger.LogInformation("[📤 Fraud Check Triggered] Published TX {TxId} to FraudDetectionService", transaction.Id);

        return Ok(new TransactionDto
        {
            Id = transaction.Id,
            FromAccountId = transaction.FromAccountId,
            ToAccountId = transaction.ToAccountId,
            Amount = transaction.Amount,
            Type = transaction.Type,
            Timestamp = transaction.Timestamp
        });
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var result = _context.Transactions.Select(t => new TransactionDto
        {
            Id = t.Id,
            FromAccountId = t.FromAccountId,
            ToAccountId = t.ToAccountId,
            Amount = t.Amount,
            Type = t.Type,
            Timestamp = t.Timestamp
        }).ToList();

        return Ok(result);
    }

    [HttpPost("unblock/{id}")]
    public async Task<IActionResult> Unblock(Guid id)
    {
        var result = await _transactionService.UnblockTransactionAsync(id);

        if (!result)
            return NotFound(new { message = "Transaction not found or already unblocked." });

        return Ok(new { message = "Transaction successfully unblocked." });
    }
}