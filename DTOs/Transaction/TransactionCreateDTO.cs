using ExpenseVista.API.DTOs.Category;
using ExpenseVista.API.Models;
using ExpenseVista.API.Models.Enums;
using ExpenseVista.API.Utilities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseVista.API.DTOs.Transaction
{
    public class TransactionCreateDTO
    {
        // Required properties
        [Required]
        [Column(TypeName = "decimal(18, 2")]
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

        public bool IsAutomatic { get; set; } = false;
        public int WalletId { get; set; }


    }
}
