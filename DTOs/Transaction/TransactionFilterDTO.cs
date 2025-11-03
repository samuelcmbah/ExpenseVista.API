using ExpenseVista.API.DTOs.Pagination;

namespace ExpenseVista.API.DTOs.Transaction
{
    public class TransactionFilterDTO : PaginationDTO
    {
        public string? SearchTerm { get; set; }  // For description/category keyword
        public string? Category { get; set; }    // Category name
        public int? Type { get; set; }           // 0 = Expense, 1 = Income
        public DateTime? StartDate { get; set; } // Optional start date
        public DateTime? EndDate { get; set; }   // Optional end date
    }
}
