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
            return Unauthorized("Invalid email or password.");

        if (!_auth.VerifyPassword(dto.Password, user.PasswordHash))
            return Unauthorized("Invalid email or password.");

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
        // Normalize role so "student", "Student", "STUDENT" all pass
        var role = dto.Role?.Trim().ToLowerInvariant();

        if (role != "admin" && role != "student")
            return BadRequest("Role must be 'Admin' or 'Student'.");

        // Admins cannot self-register — must be created by another admin
        if (role == "admin")
            return BadRequest("Admin accounts cannot be created via public registration.");

        // Email must be unique among users
        if (await _db.Users.AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower()))
            return Conflict("A user with this email already exists.");

        // Resolve the Member record by MembershipNumber or MemberId
        Member? member = null;

        if (!string.IsNullOrWhiteSpace(dto.MembershipNumber))
        {
            member = await _db.Members
                .FirstOrDefaultAsync(m => m.MembershipNumber == dto.MembershipNumber);

            if (member is null)
                return BadRequest("No member found with that membership number.");
        }
        else if (dto.MemberId is not null)
        {
            member = await _db.Members.FindAsync(dto.MemberId);

            if (member is null)
                return BadRequest("Member does not exist.");
        }
        else
        {
            return BadRequest("A membership number is required to register as a student.");
        }

        // One login per member
        if (await _db.Users.AnyAsync(u => u.MemberId == member.Id))
            return Conflict("This member already has a login account.");

        // The email on the member record must match the registration email
        if (!string.Equals(member.Email, dto.Email.Trim(), StringComparison.OrdinalIgnoreCase))
            return BadRequest("The email does not match the member record.");

        var user = new User
        {
            Email = dto.Email.Trim(),
            FullName = dto.FullName.Trim(),
            PasswordHash = _auth.HashPassword(dto.Password),
            Role = "Student",   // always stored capitalized
            IsActive = true,
            MemberId = member.Id,
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