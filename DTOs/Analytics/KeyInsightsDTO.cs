namespace ExpenseVista.API.DTOs.Analytics
{
    public class KeyInsightsDTO
    {
        public string TopSpendingCategory { get; set; } = string.Empty;
        public decimal TopSpendingAmount { get; set; }
        public int TotalTransactions { get; set; }
        public int TotalIncomeTransactions { get; set; }
        public int TotalExpenseTransactions { get; set; }
    }
}
