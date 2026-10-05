namespace Library.Core.Entities;

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Isbn {get; set; } = string.Empty;
    public int PublishedYear { get; set;}
    public int TotalCopies { get; set; }
    public int AvailableCopies { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public ICollection<Author> Authors { get; set; } = new List<Author>();
    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
}