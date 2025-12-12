using Microsoft.AspNetCore.Identity;

namespace ExpenseVista.API.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        // public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // for external login
        public string? ProviderName { get; set; }
        public string? ProviderKey { get; set; }

        //navigation properties
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public ICollection<Category> Categories { get; set; } = new List<Category>(); 
        public ICollection<Budget> Budgets { get; set; } = new List<Budget>();
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
