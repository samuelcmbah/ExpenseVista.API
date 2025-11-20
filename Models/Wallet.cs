using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseVista.API.Models
{
    public class Wallet
    {
        public int Id { get; set; }
        public string ApplicationUserId { get; set; } = string.Empty;
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Balance { get; set; } = 0m;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<WalletTransaction> Transactions { get; set; } = new();
    }

}
