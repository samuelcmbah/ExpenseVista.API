using ExpenseVista.API.DTOs.Analytics;

namespace ExpenseVista.API.Services.IServices
{
    public interface IPeriodicSummaryService
    {
        Task<PeriodicSummaryDTO> GetPeriodicSummaryAsync(string userId, string period= "This Month");
    }
}
