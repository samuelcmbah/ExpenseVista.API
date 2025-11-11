using Microsoft.AspNetCore.Identity;

namespace ExpenseVista.API.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        // public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        //navigation properties
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public ICollection<Category> Categories { get; set; } = new List<Category>(); 
        public ICollection<Budget> Budgets { get; set; } = new List<Budget>();
    }
}
