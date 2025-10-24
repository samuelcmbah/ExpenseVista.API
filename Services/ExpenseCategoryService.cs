using ExpenseVista.API.Data;
using ExpenseVista.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseVista.API.Services
{
    public class ExpenseCategoryService
    {
        private readonly ApplicationDbContext context;

        public ExpenseCategoryService(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<List<ExpenseCategory>> GetAllAsync() =>
                await context.ExpenseCategories.ToListAsync();

        public async Task<ExpenseCategory?> GetByIdAsync(int id) =>
                await context.ExpenseCategories.FindAsync(id);

        public async Task<ExpenseCategory> CreateAsync(ExpenseCategory category)
        {
            context.ExpenseCategories.Add(category);
            await context.SaveChangesAsync();
            return category;
        }

        public async Task UpdateAsync(ExpenseCategory category)
        {
            context.Entry(category).State = EntityState.Modified;
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var category = await context.ExpenseCategories.FindAsync(id);
            if (category is not null)
            {
                context.ExpenseCategories.Remove(category);
                await context.SaveChangesAsync();
            }
        }
    }
}
