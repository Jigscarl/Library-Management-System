namespace Library.Core.Entities;

public class Member
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string MembershipNumber { get; set; } = string.Empty;
    public DateTime JoinedOn { get; set; }
    public bool IsActive { get; set; }

    public ICollection<Loan> Loans { get; set; } = new List<Loan>();

    // ✅ NEW — optional inverse to User
    public User? User { get; set; }
}