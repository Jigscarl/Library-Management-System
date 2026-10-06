using System.Linq.Expressions;
using Library.Api.Dtos;
using Library.Core.Entities;
using Library.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Library.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class MembersController : ControllerBase
{
    private readonly AppDbContext _db;

    public MembersController(AppDbContext db)
    {
        _db = db;
    }

    private static readonly Expression<Func<Member, MemberDto>> ToDto = m => new MemberDto(
        m.Id,
        m.FullName,
        m.Email,
        m.MembershipNumber,
        m.JoinedOn,
        m.IsActive,
        m.Loans.Count(l => l.ReturnedOn == null));

    [HttpGet]
    public async Task<ActionResult<List<MemberDto>>> GetAll()
    {
        return await _db.Members.Select(ToDto).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MemberDto>> GetById(int id)
    {
        var member = await _db.Members.Where(m => m.Id == id).Select(ToDto).FirstOrDefaultAsync();
        if (member is null) return NotFound();
        return member;
    }

    [HttpPost]
    public async Task<ActionResult<MemberDto>> Create(CreateMemberDto dto)
    {
        if (await _db.Members.AnyAsync(m => m.Email == dto.Email))
            return Conflict("A member with this email already exists.");

        if (await _db.Members.AnyAsync(m => m.MembershipNumber == dto.MembershipNumber))
            return Conflict("A member with this membership number already exists.");

        var member = new Member
        {
            FullName = dto.FullName,
            Email = dto.Email,
            MembershipNumber = dto.MembershipNumber,
            JoinedOn = DateTime.UtcNow,
            IsActive = true
        };

        _db.Members.Add(member);
        await _db.SaveChangesAsync();

        var created = await _db.Members.Where(m => m.Id == member.Id).Select(ToDto).FirstAsync();
        return CreatedAtAction(nameof(GetById), new { id = member.Id }, created);
    }
}