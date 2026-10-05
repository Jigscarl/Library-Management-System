using System.Linq.Expressions;
using Library.Api.Dtos;
using Library.Core.Entities;
using Library.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Library.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly AppDbContext _db;

    public BooksController(AppDbContext db)
    {
        _db = db;
    }

    // One shared projection, so the mapping is written only once
    private static readonly Expression<Func<Book, BookDto>> ToDto = b => new BookDto(
        b.Id, b.Title, b.Isbn, b.PublishedYear,
        b.TotalCopies, b.AvailableCopies,
        b.Category.Name,
        b.Authors.Select(a => a.Name).ToList());

    [HttpGet]
    public async Task<ActionResult<List<BookDto>>> GetAll()
    {
        return await _db.Books.Select(ToDto).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BookDto>> GetById(int id)
    {
        var book = await _db.Books.Where(b => b.Id == id).Select(ToDto).FirstOrDefaultAsync();
        if (book is null)
        {
            return NotFound();
        }
        return book;
    }

    [HttpPost]
    public async Task<ActionResult<BookDto>> Create(CreateBookDto dto)
    {
        if (!await _db.Categories.AnyAsync(c => c.Id == dto.CategoryId))
            return BadRequest("Category does not exist.");

        if (await _db.Books.AnyAsync(b => b.Isbn == dto.Isbn))
            return Conflict("A book with this ISBN already exists.");

        var authors = await _db.Authors.Where(a => dto.AuthorIds.Contains(a.Id)).ToListAsync();
        if (authors.Count != dto.AuthorIds.Distinct().Count())
            return BadRequest("One or more authors do not exist.");

        var book = new Book
        {
            Title = dto.Title,
            Isbn = dto.Isbn,
            PublishedYear = dto.PublishedYear,
            TotalCopies = dto.TotalCopies,
            AvailableCopies = dto.TotalCopies,   // a new book starts fully available
            CategoryId = dto.CategoryId,
            Authors = authors
        };

        _db.Books.Add(book);
        await _db.SaveChangesAsync();

        var created = await _db.Books.Where(b => b.Id == book.Id).Select(ToDto).FirstAsync();
        return CreatedAtAction(nameof(GetById), new { id = book.Id }, created);
    }
}