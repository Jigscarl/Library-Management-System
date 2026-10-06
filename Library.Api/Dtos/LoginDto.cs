using System.ComponentModel.DataAnnotations;

namespace Library.Api.Dtos;

public class LoginDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(200, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;
}