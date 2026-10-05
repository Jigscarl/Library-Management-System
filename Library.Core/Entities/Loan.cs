namespace Library.Core.Entities;

public class Loan
{
    public int Id { get; set; }

    public int BookId { get; set; }
    public Book Book { get; set; } = null!;

    public int MemberId { get; set; }
    public Member Member { get; set; } = null!;

    public DateTime BorrowedOn { get; set; } = DateTime.UtcNow;    public DateTime DueOn { get; set; }
    public DateTime? ReturnedOn { get; set; }

    public Fine? Penalty { get; set; }
}