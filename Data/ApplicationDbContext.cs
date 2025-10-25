using ExpenseVista.API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace ExpenseVista.API.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Budget> Budgets { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);


            builder.Entity<Transaction>()
                .Property(t => t.Amount)
                .HasPrecision(18, 2);

            //EF Core can automatically handle these relationships via conventions, but it’s good practice to ensure clarity
            builder.Entity<Category>()
                .HasOne(c => c.ApplicationUser)
                .WithMany(u => u.Categories)
                .HasForeignKey(c => c.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Transaction>()
                .HasOne(t => t.Category)
                .WithMany(c => c.Transactions)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Transaction>()
                .HasOne(t => t.ApplicationUser)
                .WithMany(u => u.Transactions)
                .HasForeignKey(t => t.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);

            //prevents race condition problem for concurrent requests
            builder.Entity<Budget>()
                .HasIndex(b => new { b.ApplicationUserId, b.BudgetMonth })
                .IsUnique();

        }
    }
}
