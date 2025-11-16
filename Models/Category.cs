using System.ComponentModel.DataAnnotations;

namespace ExpenseVista.API.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        public string CategoryName { get; set; } = string.Empty;

        // Relationships,  Foreign key and navigation
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

        public string? ApplicationUserId { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }
        public bool IsDefault { get; set; } = false;
    }
}
