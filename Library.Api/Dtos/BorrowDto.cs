using System.ComponentModel.DataAnnotations;

namespace Library.Api.Dtos;

public class BorrowDto
{
    [Range(1, int.MaxValue)]
    public int BookId { get; set; }

    [Range(1, int.MaxValue)]
    public int MemberId { get; set; }
}