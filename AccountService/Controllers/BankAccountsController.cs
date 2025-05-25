using AccountService.Data;
using AccountService.Dtos;
using AccountService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace AccountService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BankAccountsController : ControllerBase
{
    private readonly AccountDbContext _context;

    public BankAccountsController(AccountDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAccountDto dto)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized("Missing user ID");

        var account = new BankAccount
        {
            Id = Guid.NewGuid(),
            UserId = Guid.Parse(userIdClaim),
            AccountType = dto.AccountType,
            Balance = dto.InitialDeposit,
            CreatedAt = DateTime.UtcNow
        };

        _context.BankAccounts.Add(account);
        await _context.SaveChangesAsync();

        return Ok(new BankAccountDto
        {
            Id = account.Id,
            AccountType = account.AccountType,
            Balance = account.Balance,
            CreatedAt = account.CreatedAt
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetMine()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized("Missing user ID");

        var userId = Guid.Parse(userIdClaim);

        var accounts = await _context.BankAccounts
            .Where(a => a.UserId == userId)
            .Select(a => new BankAccountDto
            {
                Id = a.Id,
                AccountType = a.AccountType,
                Balance = a.Balance,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();

        return Ok(accounts);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized("Missing user ID");

        var userId = Guid.Parse(userIdClaim);

        var account = await _context.BankAccounts
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

        if (account == null)
            return NotFound();

        return Ok(new BankAccountDto
        {
            Id = account.Id,
            AccountType = account.AccountType,
            Balance = account.Balance,
            CreatedAt = account.CreatedAt
        });
    }

    [AllowAnonymous]
    [HttpPost("update-balance")]
    public async Task<IActionResult> UpdateBalance([FromBody] BalanceUpdateDto dto)
    {
        var account = await _context.BankAccounts.FindAsync(dto.AccountId);
        if (account == null) return NotFound("Account not found");

        account.Balance += dto.Amount;
        await _context.SaveChangesAsync();

        return Ok(account);
    }
}
