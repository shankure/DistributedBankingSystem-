using LedgerService.Data;
using LedgerService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using LedgerService.Dtos;
using MassTransit;
using Shared.Messages;

namespace LedgerService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LedgerController : ControllerBase
{
    private readonly LedgerDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IPublishEndpoint _publishEndpoint;

    public LedgerController(LedgerDbContext context, IHttpClientFactory httpClientFactory, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _publishEndpoint = publishEndpoint;
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

        return CreatedAtAction(nameof(GetAllEntries), new { id = entry.Id }, entry);
    }


}
