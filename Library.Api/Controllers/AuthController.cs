using System.Security.Claims;
using Library.Api.Dtos;
using Library.Api.Services;
using Library.Core.Entities;
using Library.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Library.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IAuthService _auth;

    public AuthController(AppDbContext db, IAuthService auth)
    {
        _db = db;
        _auth = auth;
    }

    // -------- POST /api/auth/login --------

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());

        if (user is null || !user.IsActive)
        {
            return Unauthorized("Invalid email or password.");
        }

        if (!_auth.VerifyPassword(dto.Password, user.PasswordHash))
        {
            return Unauthorized("Invalid email or password.");
        }

        var (token, expiresAt) = _auth.GenerateJwt(user);

        return new AuthResponseDto(
            token,
            expiresAt,
            new AuthUserDto(user.Id, user.Email, user.FullName, user.Role, user.MemberId));
    }

    // -------- POST /api/auth/register --------

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
    {
        // Validate role
        if (dto.Role != "Admin" && dto.Role != "Student")
            return BadRequest("Role must be 'Admin' or 'Student'.");

        // Uniqueness on email
        if (await _db.Users.AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower()))
            return Conflict("A user with this email already exists.");

        // Students must have a valid MemberId; Admins must not
        if (dto.Role == "Student")
        {
            if (dto.MemberId is null)
                return BadRequest("Students require a MemberId.");

            if (!await _db.Members.AnyAsync(m => m.Id == dto.MemberId))
                return BadRequest("Member does not exist.");

            if (await _db.Users.AnyAsync(u => u.MemberId == dto.MemberId))
                return Conflict("This member already has a login account.");
        }
        else if (dto.MemberId is not null)
        {
            return BadRequest("Admins cannot be linked to a Member.");
        }

        var user = new User
        {
            Email = dto.Email.Trim(),
            FullName = dto.FullName.Trim(),
            PasswordHash = _auth.HashPassword(dto.Password),
            Role = dto.Role,
            IsActive = true,
            MemberId = dto.MemberId,
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var (token, expiresAt) = _auth.GenerateJwt(user);

        return new AuthResponseDto(
            token,
            expiresAt,
            new AuthUserDto(user.Id, user.Email, user.FullName, user.Role, user.MemberId));
    }

    // -------- GET /api/auth/me --------

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<AuthUserDto>> Me()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? User.FindFirstValue("sub");

        if (idClaim is null || !int.TryParse(idClaim, out var userId))
            return Unauthorized();

        var user = await _db.Users.FindAsync(userId);
        if (user is null || !user.IsActive)
            return Unauthorized();

        return new AuthUserDto(user.Id, user.Email, user.FullName, user.Role, user.MemberId);
    }
}