using ExpenseVista.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseVista.API.Data
{
    public class CategorySeeder
    {
        public static async Task SeedDefaultCategories(ApplicationDbContext context)
        {
            if (await context.Categories.AnyAsync(c => c.IsDefault)) return;

            var defaultCategories = new List<Category>
            {
                new Category { CategoryName = "Food & Drinks", IsDefault = true },
                new Category { CategoryName = "Shopping", IsDefault = true },
                new Category { CategoryName = "Housing", IsDefault = true },
                new Category { CategoryName = "Transportation", IsDefault = true },
                new Category { CategoryName = "Entertainment", IsDefault = true },
                new Category { CategoryName = "Health", IsDefault = true },
                new Category { CategoryName = "Salary", IsDefault = true },
                new Category { CategoryName = "Investment", IsDefault = true }
            };
            //checks if the category alrady exists before seeding
            foreach (var cat in defaultCategories)
            {
                if (!context.Categories.Any(c => c.CategoryName == cat.CategoryName && c.IsDefault))
                {
                    context.Categories.Add(cat);   // ApplicationUserId stays NULL
                }
            }
            await context.SaveChangesAsync();
        }
    }
}
