using System.ComponentModel.DataAnnotations;

namespace Library.Api.Dtos;

public class RegisterDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required, StringLength(200, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = "Student";

    // Either supply MemberId directly (admin case)
    public int? MemberId { get; set; }

    // Or supply MembershipNumber (student self-registration case)
    public string? MembershipNumber { get; set; }
}