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

        if (user.Role.Equals("Student", StringComparison.OrdinalIgnoreCase))
        {
            if (user.MemberId is null || string.IsNullOrWhiteSpace(dto.MembershipNumber))
                return Unauthorized("Invalid email, password, or membership number.");

            var membershipNumber = dto.MembershipNumber.Trim().ToUpperInvariant();
            var membershipMatches = await _db.Members.AnyAsync(m =>
                m.Id == user.MemberId && m.MembershipNumber.Trim().ToUpper() == membershipNumber);

            if (!membershipMatches)
                return Unauthorized("Invalid email, password, or membership number.");
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

        var membershipNumber = dto.MembershipNumber!.Trim().ToUpperInvariant();
        var member = await _db.Members.FirstOrDefaultAsync(m =>
            m.MembershipNumber.Trim().ToUpper() == membershipNumber);

        if (member is not null)
        {
            if (!string.Equals(member.Email, dto.Email.Trim(), StringComparison.OrdinalIgnoreCase))
                return Conflict("That membership number is already in use.");

            if (await _db.Users.AnyAsync(u => u.MemberId == member.Id))
                return Conflict("This membership number already has a login account.");
        }
        else
        {
            if (await _db.Members.AnyAsync(m => m.Email.ToLower() == dto.Email.Trim().ToLower()))
                return Conflict("This email already has a different membership number.");

            member = new Member
            {
                FullName = dto.FullName.Trim(),
                Email = dto.Email.Trim(),
                MembershipNumber = membershipNumber,
                JoinedOn = DateTime.UtcNow,
                IsActive = true,
            };
            _db.Members.Add(member);
        }

        var user = new User
        {
            Email = dto.Email.Trim(),
            FullName = dto.FullName.Trim(),
            PasswordHash = _auth.HashPassword(dto.Password),
            Role = "Student",   // always stored capitalized
            IsActive = true,
            Member = member,
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