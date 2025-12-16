using ExpenseVista.API.DTOs.Transaction;

namespace ExpenseVista.API.DTOs.Analytics
{
    public class FinancialDataDTO
    {
        public string TimePeriod { get; set; } = string.Empty;
        public SummaryDTO Summary { get; set; } = new();
        public BudgetProgressDTO BudgetProgress { get; set; } = new();
        public List<SpendingCategoryDTO> SpendingByCategory { get; set; } = new();
        public List<IncomeExpenseDataDTO> IncomeVsExpenses { get; set; } = new();
        public List<IncomeExpenseDataDTO> FinancialTrend { get; set; } = new();
        public KeyInsightsDTO keyInsights { get; set; } = new();

        //new property for file exports
        public List<TransactionDTO> Transactions { get; init; } = [];
        public List<MonthlyBudgetDetailDTO> MonthlyBudgets { get; init; } = [];
    }
}
