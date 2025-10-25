using ExpenseVista.API.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace ExpenseVista.API.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        [Required]
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        [Required]
        public TransactionType Type { get; set; }
        [Required]
        public DateTime TransactionDate { get; set; }

        // Foreign keys and navigation
        [Required]
        public int CategoryId { get; set; }
        public required Category Category { get; set; }

        [Required]
        public required string ApplicationUserId { get; set; }
        public required ApplicationUser ApplicationUser { get; set; }

    }
}
