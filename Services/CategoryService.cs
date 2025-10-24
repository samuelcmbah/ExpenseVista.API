using ExpenseVista.API.Data;
using ExpenseVista.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseVista.API.Services
{
    public class CategoryService
    {
        private readonly ApplicationDbContext context;

        public CategoryService(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<List<Category>> GetAllAsync() =>
                await context.Categories.ToListAsync();

        public async Task<Category?> GetByIdAsync(int id) =>
                await context.Categories.FindAsync(id);

        public async Task<Category> CreateAsync(Category category)
        {
            context.Categories.Add(category);
            await context.SaveChangesAsync();
            return category;
        }

        public async Task UpdateAsync(Category category)
        {
            context.Entry(category).State = EntityState.Modified;
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var category = await context.Categories.FindAsync(id);
            if (category is not null)
            {
                context.Categories.Remove(category);
                await context.SaveChangesAsync();
            }
        }
    }
}
