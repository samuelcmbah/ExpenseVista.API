namespace ExpenseVista.API.DTOs.Analytics.Exports
{
    public class FinancialReportExport
    {
        public string TimePeriod { get; init; } = string.Empty;
        public ReportOverviewExport Overview { get; init; } = new();
        public IReadOnlyList<CategorySpendingExport> CategorySpending { get; init; } = [];
        public IReadOnlyList<MonthlyIncomeExpenseExport> MonthlyIncomeVsExpenses { get; init; } = [];
        public IReadOnlyList<MonthlyIncomeExpenseExport> FinancialTrends { get; init; } = [];
    }

}
