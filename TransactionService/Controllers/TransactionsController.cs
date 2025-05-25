using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using TransactionService.Data;
using TransactionService.Dtos;
using TransactionService.Models;

namespace TransactionService.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly TransactionDbContext _context;
    private readonly HttpClient _httpClient;

    public TransactionsController(TransactionDbContext context, IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _httpClient = httpClientFactory.CreateClient();
    }

    [HttpPost]
    public async Task<IActionResult> ProcessTransaction([FromBody] CreateTransactionDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized("Missing user ID in token.");

        // Create and save transaction
        var transaction = new TransactionRecord
        {
            Id = Guid.NewGuid(),
            FromAccountId = dto.FromAccountId,
            ToAccountId = dto.ToAccountId,
            Amount = dto.Amount,
            Type = dto.Type,
            Timestamp = DateTime.UtcNow
        };

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        // Step 1: Log to LedgerService
        var ledgerEntry = new LedgerEntryDto
        {
            TransactionId = transaction.Id,
            FromAccountId = transaction.FromAccountId,
            ToAccountId = transaction.ToAccountId,
            Amount = transaction.Amount,
            Type = transaction.Type,
            Status = "COMPLETED",
            Notes = "Logged by TransactionService"
        };

        var json = JsonSerializer.Serialize(ledgerEntry);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync("http://ledger-service/api/ledger", content);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Failed to log transaction to LedgerService: {ex.Message}");
        }

        // Return confirmation
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
}
