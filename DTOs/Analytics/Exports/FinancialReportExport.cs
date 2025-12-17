namespace ExpenseVista.API.DTOs.Analytics.Exports
{
    public class FinancialReportExport
    {
        public string TimePeriod { get; init; } = string.Empty;
        public ReportOverviewExport Overview { get; init; } = new();
        public IReadOnlyList<CategorySpendingExport> CategorySpending { get; init; } = [];
        public IReadOnlyList<MonthlyIncomeExpenseExport> MonthlyIncomeVsExpenses { get; init; } = [];
        public IReadOnlyList<TransactionExport> Transactions { get; init; } = [];
        public IReadOnlyList<MonthlyBudgetExport> BudgetBreakdown { get; init; } = [];

        public string UserName { get; init; } = string.Empty;
        public string UserEmail { get; init; } = string.Empty;
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }

    }

}
