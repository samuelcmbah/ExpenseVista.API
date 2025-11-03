using ExpenseVista.API.DTOs.Analytics;

namespace ExpenseVista.API.Services.IServices
{
    public interface IAnalyticsService
    {
        Task <FinancialDataDTO>GetAnalyticsAsync(string period, string userId);
    }
}
