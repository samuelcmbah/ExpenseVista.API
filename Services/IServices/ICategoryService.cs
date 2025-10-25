using ExpenseVista.API.DTOs.Category;

namespace ExpenseVista.API.Services.IServices
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDTO>> GetAllAsync(string userId);
        Task<CategoryDTO> GetByIdAsync(int id, string userId);
        Task<CategoryDTO> CreateAsync(CreateCategoryDTO createCategoryDTO, string userId);
        Task UpdateAsync(int id, UpdateCategoryDTO dto, string userId);
        Task DeleteAsync(int id, string userId);
    }
}
