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

        [Required]
        public required string ApplicationUserId { get; set; }
        [Required]
        public required ApplicationUser ApplicationUser { get; set; }
    }
}
