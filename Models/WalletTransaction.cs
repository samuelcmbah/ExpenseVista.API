using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseVista.API.Models
{
    public class WalletTransaction
    {
        public int Id { get; set; }
        public int WalletId { get; set; }
        public Wallet Wallet { get; set; } = null!;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }
        public string Type { get; set; } = string.Empty; // "Credit" or "Debit"
        public string Source { get; set; } = string.Empty; // "Paystack", "Transfer"
        public string Reference { get; set; } = string.Empty; // Paystack reference or transfer id
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
