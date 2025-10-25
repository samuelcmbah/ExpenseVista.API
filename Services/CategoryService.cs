using AutoMapper;
using ExpenseVista.API.Data;
using ExpenseVista.API.DTOs.Category;
using ExpenseVista.API.DTOs.ExpenseCategory;
using ExpenseVista.API.Models;
using ExpenseVista.API.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace ExpenseVista.API.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public CategoryService(ApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }


        /// <summary>
        /// Retrieves a Category entity only if it belongs to the specified user.
        /// Throws KeyNotFoundException if the category is not found or unauthorized.
        /// </summary>
        private async Task<Category> GetCategoryEntityForUserAsync(int categoryId, string userId)
        {
            var category = await context.Categories
                .FirstOrDefaultAsync(c => c.Id == categoryId && c.ApplicationUserId == userId);

            if (category == null)
            {
                // Better error signaling than returning a null Category
                throw new KeyNotFoundException($"Category with ID {categoryId} not found or unauthorized.");
            }
            return category;
        }

        // --- PUBLIC SERVICE METHODS ---

        public async Task<IEnumerable<CategoryDTO>> GetAllAsync(string userId)
        {
            var categories = await context.Categories
                .Where(c => c.ApplicationUserId == userId)
                .AsNoTracking() // Performance improvement for read-only query
                .ToListAsync();

            return mapper.Map<IEnumerable<CategoryDTO>>(categories)!;
        }

        public async Task<CategoryDTO> GetByIdAsync(int id, string userId)
        {
            var category = await GetCategoryEntityForUserAsync(id, userId);

            return mapper.Map<CategoryDTO>(category)!;
        }

        public async Task<CategoryDTO> CreateAsync(CreateCategoryDTO createCategoryDTO, string userId)
        {
            var category = mapper.Map<Category>(createCategoryDTO)!;
            category.ApplicationUserId = userId;

            context.Categories.Add(category);
            await context.SaveChangesAsync();

            // Return the newly created entity mapped back to a DTO
            return mapper.Map<CategoryDTO>(category)!;
        }

        public async Task UpdateAsync(int id, UpdateCategoryDTO updateCategoryDTO, string userId)
        {
            var category = await GetCategoryEntityForUserAsync(id, userId);

            // Map DTO onto the existing entity, updating its properties
            mapper.Map(updateCategoryDTO, category);
            await context.SaveChangesAsync();
            // Note: No return value needed for a successful void update
        }

        public async Task DeleteAsync(int id, string userId)
        {
            var category = await GetCategoryEntityForUserAsync(id, userId);

            context.Categories.Remove(category);
            await context.SaveChangesAsync();
            // Note: No return value needed for a successful void delete
        }
    }
}
