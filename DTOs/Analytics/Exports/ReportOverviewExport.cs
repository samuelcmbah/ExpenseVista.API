namespace ExpenseVista.API.DTOs.Analytics.Exports
{
    //sheet 1
    public class ReportOverviewExport
    {
        public decimal TotalIncome { get; init; }
        public decimal TotalExpenses { get; init; }
        public decimal NetBalance { get; init; }

        public decimal BudgetTotal { get; init; }
        public decimal BudgetUsedPercentage { get; init; }
        public decimal BudgetBalance { get; init; }

        public string TopSpendingCategory { get; init; } = string.Empty;
        public decimal TopSpendingAmount { get; init; }

        public int TotalTransactions { get; init; }
        public int IncomeTransactions { get; init; }
        public int ExpenseTransactions { get; init; }
    }

}
