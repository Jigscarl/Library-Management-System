namespace Library.Api.Dtos;

public record BookDto(
    int Id,
    string Title,
    string Isbn,
    int PublishedYear,
    int TotalCopies,
    int AvailableCopies,
    string Category,
    List<string> Authors);