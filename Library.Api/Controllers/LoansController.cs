using System.Linq.Expressions;
using System.Security.Claims;
using Library.Api.Dtos;
using Library.Core.Common;
using Library.Core.Entities;
using Library.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Library.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LoansController : ControllerBase
{
    private readonly AppDbContext _db;

    public LoansController(AppDbContext db)
    {
        _db = db;
    }

    private static readonly Expression<Func<Loan, LoanDto>> ToDto = l => new LoanDto(
        l.Id,
        l.BookId,
        l.Book.Title,
        l.MemberId,
        l.Member.FullName,
        l.BorrowedOn,
        l.DueOn,
        l.ReturnedOn,
        l.ReturnedOn == null ? l.DueOn < DateTime.UtcNow : l.ReturnedOn > l.DueOn,
        l.Fine != null ? l.Fine.Amount : (decimal?)null,
        l.Fine != null ? l.Fine.IsPaid : (bool?)null);

    // -------- GET all (admin only) --------

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<LoanDto>>> GetAll()
    {
        return await _db.Loans.Select(ToDto).ToListAsync();
    }

    // -------- GET mine (any logged-in student) --------

    [HttpGet("mine")]
    public async Task<ActionResult<List<LoanDto>>> GetMine()
    {
        var memberId = GetCallerMemberId();
        if (memberId is null)
            return Forbid();

        return await _db.Loans
            .Where(l => l.MemberId == memberId.Value)
            .Select(ToDto)
            .ToListAsync();
    }

    // -------- GET one (owner or admin) --------

    [HttpGet("{id}")]
    public async Task<ActionResult<LoanDto>> GetById(int id)
    {
        var loan = await _db.Loans.Where(l => l.Id == id).Select(ToDto).FirstOrDefaultAsync();
        if (loan is null) return NotFound();

        if (!CanActFor(loan.MemberId))
            return Forbid();

        return loan;
    }

    // -------- BORROW --------

    [HttpPost("borrow")]
    public async Task<ActionResult<LoanDto>> Borrow(BorrowDto dto)
    {
        if (!CanActFor(dto.MemberId))
            return Forbid();

        var book = await _db.Books.FirstOrDefaultAsync(b => b.Id == dto.BookId);
        if (book is null) return NotFound($"Book {dto.BookId} not found.");

        var member = await _db.Members.FirstOrDefaultAsync(m => m.Id == dto.MemberId);
        if (member is null) return NotFound($"Member {dto.MemberId} not found.");

        if (!member.IsActive)
            return BadRequest("Member is not active.");

        if (book.AvailableCopies <= 0)
            return Conflict("No copies available for this book.");

        bool alreadyBorrowed = await _db.Loans
            .AnyAsync(l => l.BookId == dto.BookId
                        && l.MemberId == dto.MemberId
                        && l.ReturnedOn == null);
        if (alreadyBorrowed)
            return Conflict("This member already has this book on loan.");

        var now = DateTime.UtcNow;
        var loan = new Loan
        {
            BookId = dto.BookId,
            MemberId = dto.MemberId,
            BorrowedOn = now,
            DueOn = now.AddDays(LoanRules.LoanPeriodDays)
        };

        book.AvailableCopies -= 1;

        _db.Loans.Add(loan);
        await _db.SaveChangesAsync();

        var created = await _db.Loans.Where(l => l.Id == loan.Id).Select(ToDto).FirstAsync();
        return CreatedAtAction(nameof(GetById), new { id = loan.Id }, created);
    }

    // -------- RETURN --------

    [HttpPost("{id}/return")]
    public async Task<ActionResult<LoanDto>> Return(int id)
    {
        var loan = await _db.Loans
            .Include(l => l.Book)
            .Include(l => l.Fine)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (loan is null) return NotFound();

        if (!CanActFor(loan.MemberId))
            return Forbid();

        if (loan.ReturnedOn != null)
            return Conflict("This loan has already been returned.");

        var now = DateTime.UtcNow;
        loan.ReturnedOn = now;
        loan.Book.AvailableCopies += 1;

        if (now > loan.DueOn)
        {
            int daysLate = (int)Math.Ceiling((now - loan.DueOn).TotalDays);
            loan.Fine = new Fine
            {
                Amount = daysLate * LoanRules.FinePerDay,
                IsPaid = false
            };
        }

        await _db.SaveChangesAsync();

        var updated = await _db.Loans.Where(l => l.Id == id).Select(ToDto).FirstAsync();
        return updated;
    }

    // -------- Auth helpers --------

    private bool IsAdmin() => User.IsInRole("Admin");

    private int? GetCallerMemberId()
    {
        var claim = User.FindFirstValue("memberId");
        return int.TryParse(claim, out var id) ? id : null;
    }

    private bool CanActFor(int memberId)
    {
        if (IsAdmin()) return true;
        var callerId = GetCallerMemberId();
        return callerId.HasValue && callerId.Value == memberId;
    }
}