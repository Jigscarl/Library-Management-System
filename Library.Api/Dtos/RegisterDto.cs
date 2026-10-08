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

    [Required, RegularExpression(@"^[A-Z]{3}/B/\d{2}-\d{5}/\d{4}$", ErrorMessage = "Membership number must match XXX/B/NN-NNNNN/YYYY (for example, COM/B/01-00132/2023).")]
    public string? MembershipNumber { get; set; }
}