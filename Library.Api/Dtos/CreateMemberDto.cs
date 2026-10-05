using System.ComponentModel.DataAnnotations;

namespace Library.Api.Dtos;

public class CreateMemberDto
{
    [Required, StringLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required, StringLength(200), EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string MembershipNumber { get; set; } = string.Empty;
}