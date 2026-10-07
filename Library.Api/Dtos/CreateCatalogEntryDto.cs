using System.ComponentModel.DataAnnotations;

namespace Library.Api.Dtos;

public class CreateCatalogEntryDto
{
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;
}