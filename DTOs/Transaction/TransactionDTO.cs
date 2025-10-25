using ExpenseVista.API.DTOs.Category;

namespace ExpenseVista.API.DTOs.Transaction
{
    public class TransactionDTO
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public DateTime TransactionDate { get; set; }
        public CategoryDTO Category { get; set; } = new();

    }
}
