namespace ExpenseVista.API.DTOs.Analytics.Exports
{
    //Sheet 2
    public class CategorySpendingExport
    {
        public string Category { get; init; } = string.Empty;
        public decimal AmountSpent { get; init; }
        public decimal Percentage { get; init; }
    }
}
