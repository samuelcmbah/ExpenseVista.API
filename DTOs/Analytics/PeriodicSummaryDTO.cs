using ExpenseVista.API.DTOs.Transaction;

namespace ExpenseVista.API.DTOs.Analytics
{
    public class PeriodicSummaryDTO
    {
        public DateTime Period { get; set; }
        public List<TransactionDTO>? Transactions { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
    }

}
