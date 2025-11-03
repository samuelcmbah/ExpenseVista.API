namespace ExpenseVista.API.DTOs.Analytics
{
    public class IncomeExpenseDataDTO
    {
        public string Month { get; set; } = string.Empty;
        public decimal Income { get; set; }
        public decimal Expenses { get; set; }
    }
}
