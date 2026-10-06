namespace Library.Api.Dtos;

public record AuthResponseDto(
    string Token,
    DateTime ExpiresAt,
    AuthUserDto User);

public record AuthUserDto(
    int Id,
    string Email,
    string FullName,
    string Role,
    int? MemberId);