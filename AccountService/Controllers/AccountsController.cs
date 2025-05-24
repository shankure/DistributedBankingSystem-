using AccountService.Data;
using AccountService.Models;
using Microsoft.AspNetCore.Mvc;

namespace AccountService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly AccountDbContext _context;

    public AccountsController(AccountDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult GetAllAccounts() => Ok(_context.BankAccounts.ToList());

    [HttpPost]
    public IActionResult CreateAccount(BankAccount account)
    {
        account.Id = Guid.NewGuid();
        _context.BankAccounts.Add(account);
        _context.SaveChanges();
        return CreatedAtAction(nameof(GetAllAccounts), new { id = account.Id }, account);
    }
}
