using LedgerService.Data;
using LedgerService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using LedgerService.Dtos;

namespace LedgerService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LedgerController : ControllerBase
{
    private readonly LedgerDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;

    public LedgerController(LedgerDbContext context, IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet]
    public IActionResult GetAllEntries() => Ok(_context.LedgerEntries.ToList());

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> LogEntry(LedgerEntry entry)
    {
        entry.Id = Guid.NewGuid();
        entry.Timestamp = DateTime.UtcNow;

        _context.LedgerEntries.Add(entry);
        _context.SaveChanges();

        // Send updates to AccountService
        var httpClient = _httpClientFactory.CreateClient("AccountService");

        var updates = new List<BalanceUpdateDto>();

        if (entry.FromAccountId != null)
        {
            // Subtract from sender
            updates.Add(new BalanceUpdateDto
            {
                AccountId = entry.FromAccountId.Value,
                Amount = -entry.Amount
            });
        }

        if (entry.ToAccountId != null)
        {
            // Add to receiver
            updates.Add(new BalanceUpdateDto
            {
                AccountId = entry.ToAccountId.Value,
                Amount = entry.Amount
            });
        }

        foreach (var update in updates)
        {
            var json = JsonSerializer.Serialize(update);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("http://account-service/api/bankaccounts/update-balance", content);
            response.EnsureSuccessStatusCode(); // will throw if failed
        }

        return CreatedAtAction(nameof(GetAllEntries), new { id = entry.Id }, entry);
    }

}
