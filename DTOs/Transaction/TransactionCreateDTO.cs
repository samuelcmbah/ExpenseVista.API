using ExpenseVista.API.DTOs.Category;
using ExpenseVista.API.Models;
using ExpenseVista.API.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace ExpenseVista.API.DTOs.Transaction
{
    public class TransactionCreateDTO
    {
        // Required properties
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be positive.")]
        public decimal Amount { get; set; }

        [Required]
        public TransactionType Type { get; set; }

        [Required]
        public DateTime TransactionDate { get; set; }

        // Foreign Key: Use the ID for input
        [Required]
        public int CategoryId { get; set; }

        // Optional properties
        public string? Description { get; set; }
    }
}
