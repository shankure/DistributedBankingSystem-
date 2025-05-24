using Microsoft.AspNetCore.Mvc;
using UserService.Data;
using UserService.Models;

namespace UserService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserDbContext _context;

    public UsersController(UserDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult GetAllUsers() => Ok(_context.Users.ToList());

    [HttpPost]
    public IActionResult Register(User user)
    {
        user.Id = Guid.NewGuid();
        _context.Users.Add(user);
        _context.SaveChanges();
        return CreatedAtAction(nameof(GetAllUsers), new { id = user.Id }, user);
    }
}