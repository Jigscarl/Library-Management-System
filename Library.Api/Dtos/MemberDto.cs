namespace Library.Api.Dtos;

public record MemberDto(
    int Id,
    string FullName,
    string Email,
    string MembershipNumber,
    DateTimeOffset JoinedOn,
    bool IsActive,
    int ActiveLoans);