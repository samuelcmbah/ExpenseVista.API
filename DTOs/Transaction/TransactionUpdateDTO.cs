using ExpenseVista.API.DTOs.Category;
using ExpenseVista.API.Models.Enums;
using ExpenseVista.API.Utilities;
using System.ComponentModel.DataAnnotations;

namespace ExpenseVista.API.DTOs.Transaction
{
    public class TransactionUpdateDTO
    {
        // Ensure the ID is present for mapping/lookup
        [Required]
        public int Id { get; set; }

        // Required properties
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be positive.")]
        public decimal Amount { get; set; }

        [Required]
        public TransactionType Type { get; set; }

        [Required]
        [NotInFuture]
        public DateTime TransactionDate { get; set; }

        // Foreign Key: Use the ID for input
        [Required]
        public int CategoryId { get; set; }

        // Optional properties
        public string? Description { get; set; }
    }
}
