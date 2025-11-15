namespace ExpenseVista.API.DTOs.Budget
{
    public class BudgetStatusDTO
    {
        public int Id { get; set; }
        public decimal MonthlyLimit { get; set; }
        public DateTime BudgetMonth { get; set; }

        // Optional: Include a property for the currently calculated usage for display
        public decimal TotalIncome { get; set; }
        public decimal CurrentUsage { get; set; }
        public decimal RemainingAmount { get; set; }
        public decimal PercentageUsed { get; set; }
        public bool BudgetSet { get; internal set; }
    }
}
