using ExpenseVista.API.DTOs.Analytics;

namespace ExpenseVista.API.Services.IServices
{
    public interface IDashboardService
    {
        Task<DashboardDTO> GetDashboardAsync(string userId, DateTime month);
    }
}
