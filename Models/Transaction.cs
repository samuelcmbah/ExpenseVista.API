namespace ExpenseVista.API.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public DateTime TransactionDate { get; set; }

        // Foreign keys and navigation
        public int CategoryId { get; set; }
        public required Category Category { get; set; }

        public required string ApplicationUserId { get; set; }
        public required ApplicationUser ApplicationUser { get; set; }

    }
}
