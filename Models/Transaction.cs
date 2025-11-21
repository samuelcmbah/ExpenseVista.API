using ExpenseVista.API.Models.Enums;
using ExpenseVista.API.Utilities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseVista.API.Models
{
    public class Transaction
    {
        public int Id { get; set; }

        private decimal amount;
        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount
        {
            //ensure all values are normalized before saving
            get => amount;
            set => amount = Math.Round(value, 2);
        }

        public string? Description { get; set; }
        [Required]
        public TransactionType Type { get; set; }
        [Required]
        [NotInFuture]
        public DateTime TransactionDate { get; set; }

        // Foreign keys and navigation
        [Required]
        public int CategoryId { get; set; }
        public required Category Category { get; set; }

        [Required]
        public required string ApplicationUserId { get; set; }
        public required ApplicationUser ApplicationUser { get; set; }

        //currency exchange
        public string Currency { get; set; } = "NGN";
        [Column(TypeName = "decimal(18, 6)")]
        public decimal ExchangeRate { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal ConvertedAmount { get; set; }

    }
}
