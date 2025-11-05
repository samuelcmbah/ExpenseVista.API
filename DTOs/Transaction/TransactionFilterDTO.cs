using ExpenseVista.API.DTOs.Pagination;

namespace ExpenseVista.API.DTOs.Transaction
{
    public class FilterPagedTransactionDTO : PaginationDTO
    {
        public string? SearchTerm { get; set; }  // For description/category keyword
        public string? CategoryName { get; set; }    // Category name
        public int? Type { get; set; }           // 0 = Expense, 1 = Income
        public DateTime? StartDate { get; set; } // Optional start date
        public DateTime? EndDate { get; set; }   // Optional end date
    }
}
