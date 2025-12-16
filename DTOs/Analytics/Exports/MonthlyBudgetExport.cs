namespace ExpenseVista.API.DTOs.Analytics.Exports
{
    public class MonthlyBudgetExport
    {
        public string Month { get; init; } = string.Empty;
        public decimal? Budget { get; init; } // Nullable, for months with no budget
        public decimal Spent { get; init; }
        public decimal? Balance { get; init; } // Nullable, for months with no budget
    }
}
