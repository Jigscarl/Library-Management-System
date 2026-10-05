using System.ComponentModel.DataAnnotations;

namespace Library.Api.Dtos;

public class UpdateBookDto
{
    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string Isbn { get; set; } = string.Empty;

    [Range(1000, 2100)]
    public int PublishedYear { get; set; }

    [Range(1, 1000)]
    public int TotalCopies { get; set; }

    public int CategoryId { get; set; }
}