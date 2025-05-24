using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using TransactionService.Data;
using TransactionService.Models;
using TransactionService.DTOs;

namespace TransactionService.Controllers;

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
    public async Task<IActionResult> ProcessTransaction(TransactionRecord transaction)
    {
        transaction.Id = Guid.NewGuid();
        transaction.Timestamp = DateTime.UtcNow;

        _context.Transactions.Add(transaction);
        _context.SaveChanges();

        // STEP 1: Send to LedgerService
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

        return CreatedAtAction(nameof(GetAll), new { id = transaction.Id }, transaction);
    }

    [HttpGet]
    public IActionResult GetAll() => Ok(_context.Transactions.ToList());
}
