using LedgerService.Data;
using LedgerService.Models;
using Microsoft.AspNetCore.Mvc;

namespace LedgerService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LedgerController : ControllerBase
{
    private readonly LedgerDbContext _context;

    public LedgerController(LedgerDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult GetAllEntries() => Ok(_context.LedgerEntries.ToList());

    [HttpPost]
    public IActionResult LogEntry(LedgerEntry entry)
    {
        entry.Id = Guid.NewGuid();
        entry.Timestamp = DateTime.UtcNow;

        _context.LedgerEntries.Add(entry);
        _context.SaveChanges();

        return CreatedAtAction(nameof(GetAllEntries), new { id = entry.Id }, entry);
    }
}