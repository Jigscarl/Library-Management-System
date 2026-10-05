using Library.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Library.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Book> Books => Set<Book>();
    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Loan> Loans => Set<Loan>();
    public DbSet<Fine> Fines => Set<Fine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // --- Delete behavior: protect data from accidental cascade wipes ---

        modelBuilder.Entity<Book>()
            .HasOne(b => b.Category)
            .WithMany(c => c.Books)
            .HasForeignKey(b => b.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Loan>()
            .HasOne(l => l.Book)
            .WithMany(b => b.Loans)
            .HasForeignKey(l => l.BookId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Loan>()
            .HasOne(l => l.Member)
            .WithMany(m => m.Loans)
            .HasForeignKey(l => l.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

        // --- Uniqueness constraints ---

        modelBuilder.Entity<Member>().HasIndex(m => m.Email).IsUnique();
        modelBuilder.Entity<Member>().HasIndex(m => m.MembershipNumber).IsUnique();
        modelBuilder.Entity<Book>().HasIndex(b => b.Isbn).IsUnique();

        // --- Precision for money ---

        modelBuilder.Entity<Fine>().Property(f => f.Amount).HasPrecision(18, 2);
    }
}