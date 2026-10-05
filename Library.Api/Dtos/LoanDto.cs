namespace Library.Api.Dtos;

public record LoanDto(
    int Id,
    int BookId,
    string BookTitle,
    int MemberId,
    string MemberName,
    DateTime BorrowedOn,
    DateTime DueOn,
    DateTime? ReturnedOn,
    bool IsOverdue,
    decimal? FineAmount,
    bool? FineIsPaid);