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

    // "Admin" or "Student"
    [Required]
    public string Role { get; set; } = "Student";

    // Required for students — links this user to a Member record
    public int? MemberId { get; set; }
}