using GamblingApp.Models.DTOs;
using GamblingApp.Models.Entities;
using GamblingApp.Services.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GamblingApp.Controllers;

/// <summary>
/// Handles all user-related operations such as registration, login,
/// profile lookups, and basic account management actions.  
/// This controller is the core entry point for authentication and
/// general user data retrieval within the application.
/// </summary>

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserRepository _userRepo;

    public UserController(IUserRepository userRepo)
    {
        _userRepo = userRepo;
    }

    /// <summary>
    /// Maps User entiy to the DTO 
    /// </summary>
    private static UserReadDto ToReadDto(User u) => new()
    {
        Id = u.Id,
        FirstName = u.FirstName,
        LastName = u.LastName,
        Email = u.Email,
        DemoBalance = u.DemoBalance,
        IsAdmin = u.IsAdmin,
        CreatedAt = u.CreatedAt
    };

    /// <summary>
    /// Registers a new user account. Performs email uniqueness checks
    /// and sets an initial balance for demo play.
    /// </summary>

    // POST: /api/user/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromForm] UserRegisterDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var exists = await _userRepo.EmailExistsAsync(dto.Email);
        if (exists)
        {
            return Conflict(new { message = "Email already in use." });
        }

        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Password = dto.Password, // plain text for this project only
            DemoBalance = 1000m,     // starting credits
            IsAdmin = false
        };

        var created = await _userRepo.CreateAsync(user);

        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id },
            ToReadDto(created));
    }

    /// <summary>
    /// Logs in a user by verifying email + password.
    /// Returns the user's public data if authentication succeeds.
    /// </summary>

    // POST: /api/user/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromForm] UserLoginDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await _userRepo.ReadByEmailAsync(dto.Email);
        if (user == null || user.Password != dto.Password)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        return Ok(ToReadDto(user));
    }

    /// <summary>
    /// Retrieves a user's profile data by Id.
    /// </summary>

    // GET: /api/user/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _userRepo.ReadAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        return Ok(ToReadDto(user));
    }

    /// <summary>
    /// Retrieves a user's profile using their email address.
    /// </summary>

    // GET: /api/user/by-email?email=...
    [HttpGet("by-email")]
    public async Task<IActionResult> GetByEmail([FromQuery] string email)
    {
        var user = await _userRepo.ReadByEmailAsync(email);
        if (user == null)
        {
            return NotFound();
        }

        return Ok(ToReadDto(user));
    }

    /// <summary>
    /// Returns only a user's current balance.  
    /// Used by game views to refresh balance via AJAX.
    /// </summary>

    // GET: /api/user/{id}/balance
    [HttpGet("{id:int}/balance")]
    public async Task<IActionResult> GetBalance(int id)
    {
        var user = await _userRepo.ReadAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        return Ok(new { user.Id, user.DemoBalance });
    }

    /// <summary>
    /// Allows a user to update their email after confirming
    /// their current password.
    /// </summary>

    // PUT: /api/user/{id}/email
    [HttpPut("{id:int}/email")]
    public async Task<IActionResult> UpdateEmail(int id, [FromForm] UpdateEmailDto dto)
    {
        var user = await _userRepo.ReadAsync(id);
        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

    // using plain text passwords, so just compare directly
        if (!string.Equals(user.Password, dto.CurrentPassword))
        {
            return Unauthorized(new { message = "Current password is incorrect." });
        }

    // prevent duplicate emails
        var existing = await _userRepo.ReadByEmailAsync(dto.NewEmail);
        if (existing != null && existing.Id != user.Id)
        {
            return Conflict(new { message = "Email is already in use by another account." });
        }

        user.Email = dto.NewEmail;
        await _userRepo.UpdateAsync(user.Id, user);

        return Ok(new { message = "Email updated successfully.", email = user.Email });
}


    /// <summary>
    /// Allows a user to update their password after confirming
    /// their current password.
    /// </summary>

    // PUT: /api/user/{id}/password
    [HttpPut("{id:int}/password")]
    public async Task<IActionResult> UpdatePassword(int id, [FromForm] UpdatePasswordDto dto)
    {
        var user = await _userRepo.ReadAsync(id);
        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

        if (!string.Equals(user.Password, dto.CurrentPassword))
        {
            return Unauthorized(new { message = "Current password is incorrect." });
        }

        user.Password = dto.NewPassword;
        await _userRepo.UpdateAsync(user.Id, user);

        return Ok(new { message = "Password updated successfully." });
    }

    /// <summary>
    /// Admin-only: updates a user's email without requiring a password.
    /// </summary>

    // PUT: /api/user/{id}/admin-email
    [HttpPut("{id:int}/admin-email")]
    public async Task<IActionResult> AdminUpdateEmail(
        int id,
        [FromForm] string NewEmail)
    {
        if (string.IsNullOrWhiteSpace(NewEmail))
        {
            return BadRequest(new { message = "NewEmail is required." });
        }

        var user = await _userRepo.ReadAsync(id);
        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

    // prevent duplicate emails
        var existing = await _userRepo.ReadByEmailAsync(NewEmail);
        if (existing != null && existing.Id != user.Id)
        {
            return Conflict(new { message = "Email is already in use by another account." });
        }

        user.Email = NewEmail;
        await _userRepo.UpdateAsync(user.Id, user);

        return Ok(new
        {
            message = "Email updated successfully.",
            user = ToReadDto(user)
        });
    }


    /// <summary>
    /// Returns all users. This is mainly useful for admin pages.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userRepo.ReadAllAsync();
        var dtos = users.Select(ToReadDto).ToList();
        return Ok(dtos);
    }


    /// <summary>
    /// Admin-only: toggles a user's admin status.
    /// </summary>

// PUT: /api/user/{id}/admin-role
    [HttpPut("{id:int}/admin-role")]
    public async Task<IActionResult> AdminUpdateRole(
        int id,
        [FromForm] bool IsAdmin)
    {
        var user = await _userRepo.ReadAsync(id);
        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

        user.IsAdmin = IsAdmin;
        await _userRepo.UpdateAsync(user.Id, user);

        return Ok(new
        {
            message = "Admin role updated.",
            user = ToReadDto(user)
        });
    }
}