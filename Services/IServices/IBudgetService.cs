using ExpenseVista.API.DTOs.Budget;

namespace ExpenseVista.API.Services.IServices
{
    public interface IBudgetService
    {
        // CRUD Operations
        Task<BudgetDTO> CreateAsync(BudgetCreateDTO budgetCreateDTO, string userId);
        Task UpdateAsync(int id, BudgetUpdateDTO budgetUpdateDTO, string userId);
        Task DeleteAsync(int id, string userId);

        // Read/Calculation Operations
        Task<BudgetDTO> GetBudgetStatusForMonthAsync(DateTime month, string userId);
        Task<IEnumerable<BudgetDTO>> GetAllBudgetsAsync(string userId);
    }
}