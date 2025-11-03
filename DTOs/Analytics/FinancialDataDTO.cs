namespace ExpenseVista.API.DTOs.Analytics
{
    public class FinancialDataDTO
    {
        public string TimePeriod { get; set; } = string.Empty;
        public BudgetProgressDTO BudgetProgress { get; set; } = new();
        public List<SpendingCategoryDTO> SpendingByCategory { get; set; } = new();
        public List<IncomeExpenseDataDTO> IncomeVsExpenses { get; set; } = new();
        public List<IncomeExpenseDataDTO> FinancialTrend { get; set; } = new();
        public KeyInsightsDTO keyInsights { get; set; } = new();
    }
}
