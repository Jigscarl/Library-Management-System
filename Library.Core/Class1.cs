namespace Library.Core.Entities;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<Book> Books { get; set; } = new List<Book>();
}

public class Author
{
    public int Id { get; set; }
    public string Name {get; set; } = string.Empty;

    public ICollection<Book> Books { get; set; } = new List<Book>();
}

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = sring.Empty;
    public string Isbn {get; set; } = string.Empty;
    public int PublishedYear { get; set;}
    public int TotalCopies { get; set; }
    public int AvailableCopies { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public ICollection<Author> Authors { get; set; } = new List<Author>();
}