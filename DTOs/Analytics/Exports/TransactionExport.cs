namespace ExpenseVista.API.DTOs.Analytics.Exports
{
    public class TransactionExport
    {
        public DateTime Date { get; init; }
        public string Description { get; init; } = string.Empty;
        public string Category { get; init; } = string.Empty;
        public decimal? Income { get; init; }
        public decimal? Expense { get; init; }
    }
}
