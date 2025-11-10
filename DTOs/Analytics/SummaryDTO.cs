namespace ExpenseVista.API.DTOs.Analytics
{
    public class SummaryDTO
    {
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetBalance { get; set; }
        public decimal SavingsRate { get; set; }
    }
}
