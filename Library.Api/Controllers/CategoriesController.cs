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
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _db;

    public CategoriesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<CatalogEntryDto>>> GetAll()
    {
        return await _db.Categories
            .OrderBy(category => category.Name)
            .Select(category => new CatalogEntryDto(category.Id, category.Name))
            .ToListAsync();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CatalogEntryDto>> Create(CreateCatalogEntryDto dto)
    {
        var name = dto.Name.Trim();
        if (name.Length == 0)
            return BadRequest("Category name is required.");

        if (await _db.Categories.AnyAsync(category => category.Name.ToLower() == name.ToLower()))
            return Conflict("A category with this name already exists.");

        var category = new Category { Name = name };
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        var created = new CatalogEntryDto(category.Id, category.Name);
        return CreatedAtAction(nameof(GetAll), created);
    }
}