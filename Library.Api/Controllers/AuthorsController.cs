using Library.Api.Dtos;
using Library.Core.Entities;
using Library.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Library.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AuthorsController : ControllerBase
{
    private readonly AppDbContext _db;

    public AuthorsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<CatalogEntryDto>>> GetAll()
    {
        return await _db.Authors
            .OrderBy(author => author.Name)
            .Select(author => new CatalogEntryDto(author.Id, author.Name))
            .ToListAsync();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CatalogEntryDto>> Create(CreateCatalogEntryDto dto)
    {
        var name = dto.Name.Trim();
        if (name.Length == 0)
            return BadRequest("Author name is required.");

        if (await _db.Authors.AnyAsync(author => author.Name.ToLower() == name.ToLower()))
            return Conflict("An author with this name already exists.");

        var author = new Author { Name = name };
        _db.Authors.Add(author);
        await _db.SaveChangesAsync();

        var created = new CatalogEntryDto(author.Id, author.Name);
        return CreatedAtAction(nameof(GetAll), created);
    }
}