namespace ExpenseVista.API.DTOs.Analytics
{
    public class MonthlyBudgetDetailDTO
    {
        public string Month { get; set; } = string.Empty;
        public decimal? BudgetAmount { get; set; } // Nullable
        public decimal AmountSpent { get; set; }
    }
}
