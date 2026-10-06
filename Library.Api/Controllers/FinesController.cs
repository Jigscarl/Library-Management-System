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
public class FinesController : ControllerBase
{
    private readonly AppDbContext _db;

    public FinesController(AppDbContext db)
    {
        _db = db;
    }

    private static readonly Expression<Func<Fine, FineDto>> ToDto = f => new FineDto(
        f.Id,
        f.LoanId,
        f.Loan.MemberId,
        f.Loan.Member.FullName,
        f.Loan.Book.Title,
        f.Amount,
        f.IsPaid,
        f.Loan.BorrowedOn,
        f.Loan.DueOn,
        f.Loan.ReturnedOn);

    [HttpGet]
    public async Task<ActionResult<List<FineDto>>> GetAll([FromQuery] bool unpaidOnly = false)
    {
        var query = _db.Fines.AsQueryable();
        if (unpaidOnly)
            query = query.Where(f => !f.IsPaid);

        return await query.Select(ToDto).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<FineDto>> GetById(int id)
    {
        var fine = await _db.Fines.Where(f => f.Id == id).Select(ToDto).FirstOrDefaultAsync();
        if (fine is null) return NotFound();
        return fine;
    }

    [HttpGet("member/{memberId}")]
    public async Task<ActionResult<List<FineDto>>> GetByMember(int memberId)
    {
        if (!await _db.Members.AnyAsync(m => m.Id == memberId))
            return NotFound("Member not found.");

        return await _db.Fines
            .Where(f => f.Loan.MemberId == memberId)
            .Select(ToDto)
            .ToListAsync();
    }

    [HttpPut("{id}/pay")]
    public async Task<ActionResult<FineDto>> Pay(int id)
    {
        var fine = await _db.Fines.FirstOrDefaultAsync(f => f.Id == id);
        if (fine is null) return NotFound();

        if (fine.IsPaid)
            return Conflict("This fine has already been paid.");

        fine.IsPaid = true;
        await _db.SaveChangesAsync();

        var updated = await _db.Fines.Where(f => f.Id == id).Select(ToDto).FirstAsync();
        return updated;
    }
}