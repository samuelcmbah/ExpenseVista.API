using ExpenseVista.API.DTOs.Budget;

namespace ExpenseVista.API.DTOs.Analytics
{
    public class DashboardDTO
    {
        public PeriodicSummaryDTO Summary { get; set; } = new();
        public BudgetStatusDTO? Budget { get; set; } = new();
    }

}
