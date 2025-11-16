using AutoMapper;
using ExpenseVista.API.Data;
using ExpenseVista.API.DTOs.Category;
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

        // --------------------------------------------------------------------
        // INTERNAL HELPERS
        // --------------------------------------------------------------------

        /// <summary>
        /// Retrieves a Category entity for the given ID.
        /// User-owned categories must match the user; default categories are allowed.
        /// </summary>
        private async Task<Category> GetCategoryEntityAsync(int categoryId, string userId)
        {
            var category = await context.Categories
                .FirstOrDefaultAsync(c => c.Id == categoryId);

            if (category == null)
                throw new KeyNotFoundException($"Category with ID {categoryId} not found.");

            // User trying to access another user's custom category
            if (!category.IsDefault && category.ApplicationUserId != userId)
                throw new UnauthorizedAccessException("You do not have permission to access this category.");

            return category;
        }

        /// <summary>
        /// Ensures a category is NOT a default category.
        /// Used before update/delete operations.
        /// </summary>
        private static void EnsureNotDefault(Category category)
        {
            if (category.IsDefault)
                throw new InvalidOperationException("Default categories cannot be modified or deleted.");
        }

        private async Task<bool> HasLinkedTransactionsAsync(int categoryId, string userId)
        {
            return await context.Transactions
                .AnyAsync(t => t.CategoryId == categoryId && t.ApplicationUserId == userId);
        }

        // --------------------------------------------------------------------
        // PUBLIC SERVICE METHODS
        // --------------------------------------------------------------------

        public async Task<IEnumerable<CategoryDTO>> GetAllAsync(string userId)
        {
            var categories = await context.Categories
                .Where(c => c.ApplicationUserId == userId || c.IsDefault)
                .AsNoTracking()
                .OrderBy(c => c.CategoryName)
                .ToListAsync();

            return mapper.Map<IEnumerable<CategoryDTO>>(categories)!;
        }

        public async Task<CategoryDTO> GetByIdAsync(int id, string userId)
        {
            var category = await GetCategoryEntityAsync(id, userId);
            return mapper.Map<CategoryDTO>(category)!;
        }

        public async Task<CategoryDTO> CreateAsync(CreateCategoryDTO dto, string userId)
        {
            var newName = dto.CategoryName.Trim().ToLower();

            // Check default categories
            bool existsAsDefault = await context.Categories
                .AnyAsync(c =>
                    c.IsDefault &&
                    c.CategoryName.ToLower() == newName);

            if (existsAsDefault)
                throw new InvalidOperationException("This category already exists as a default category.");

            // Check user's custom categories
            bool existsForUser = await context.Categories
                .AnyAsync(c =>
                    !c.IsDefault &&
                    c.ApplicationUserId == userId &&
                    c.CategoryName.ToLower() == newName);

            if (existsForUser)
                throw new InvalidOperationException("You already have a custom category with this name.");

            // Create category
            var category = mapper.Map<Category>(dto)!;
            category.ApplicationUserId = userId;
            category.IsDefault = false;

            context.Categories.Add(category);
            await context.SaveChangesAsync();

            return mapper.Map<CategoryDTO>(category)!;
        }


        public async Task UpdateAsync(int id, UpdateCategoryDTO dto, string userId)
        {
            var category = await GetCategoryEntityAsync(id, userId);

            EnsureNotDefault(category); // Default categories cannot be changed

            mapper.Map(dto, category);

            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id, string userId)
        {
            var category = await GetCategoryEntityAsync(id, userId);

            EnsureNotDefault(category); // Prevent deleting default categories

            if (await HasLinkedTransactionsAsync(id, userId))
                throw new InvalidOperationException("Cannot delete category because it has linked transactions.");

            context.Categories.Remove(category);
            await context.SaveChangesAsync();
        }
    }
}
