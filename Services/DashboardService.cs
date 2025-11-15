using ExpenseVista.API.DTOs.Analytics;
using ExpenseVista.API.Services.IServices;

namespace ExpenseVista.API.Services
{
    public class DashboardService: IDashboardService
    {
        private readonly IPeriodicSummaryService periodicSummaryService;
        private readonly IBudgetService budgetService;

        public DashboardService(
            IPeriodicSummaryService periodicSummaryService,
            IBudgetService budgetService)
        {
            this.periodicSummaryService = periodicSummaryService;
            this.budgetService = budgetService;
        }

        public async Task<DashboardDTO> GetDashboardAsync(string userId, DateTime month)
        {
            var summary = await periodicSummaryService.GetPeriodicSummaryAsync(userId);
            var budget = await budgetService.GetBudgetStatusForMonthAsync(userId);

            return new DashboardDTO
            {
                Summary = summary,
                Budget = budget
            };
        }
    }
}
