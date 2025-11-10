namespace ExpenseVista.API.DTOs.Analytics
{
    public class FinancialTransactionAnalytics
    {
        public SummaryDTO Summary { get; set; } = new();
        public List<SpendingCategoryDTO> SpendingByCategory { get; set; } = new();
        public List<IncomeExpenseDataDTO> IncomeVsExpenses { get; set; } = new();
        public List<IncomeExpenseDataDTO> FinancialTrend { get; set; } = new();
        public KeyInsightsDTO KeyInsights { get; set; } = new();
    }
}
