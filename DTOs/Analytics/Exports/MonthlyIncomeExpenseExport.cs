namespace ExpenseVista.API.DTOs.Analytics.Exports
{
    //Sheets 3 & 4
    public class MonthlyIncomeExpenseExport
    {
        public string Month { get; init; } = string.Empty;
        public decimal Income { get; init; }
        public decimal Expenses { get; init; }
    }

}
