namespace Library.Api.Dtos;

public record FineDto(
    int Id,
    int LoanId,
    int MemberId,
    string MemberName,
    string BookTitle,
    decimal Amount,
    bool IsPaid,
    DateTime BorrowedOn,
    DateTime DueOn,
    DateTime? ReturnedOn);