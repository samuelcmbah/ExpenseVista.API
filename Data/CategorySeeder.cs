using ExpenseVista.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseVista.API.Data
{
    public class CategorySeeder
    {
        public static async Task EnsurePopulatedAsync(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var services = scope.ServiceProvider;

            var context = services.GetRequiredService<ApplicationDbContext>();

            var defaultCategories = new List<Category>
            {
                new Category { CategoryName = "Food & Drinks", IsDefault = true },
                new Category { CategoryName = "Shopping", IsDefault = true },
                new Category { CategoryName = "Housing", IsDefault = true },
                new Category { CategoryName = "Transportation", IsDefault = true },
                new Category { CategoryName = "Computing & Internet", IsDefault = true },
                new Category { CategoryName = "Entertainment", IsDefault = true },
                new Category { CategoryName = "Health", IsDefault = true },
                new Category { CategoryName = "Salary", IsDefault = true },
                new Category { CategoryName = "Betting", IsDefault = true },
                new Category { CategoryName = "Investment", IsDefault = true },
                new Category { CategoryName = "Funding", IsDefault = true },
                new Category { CategoryName = "Transfers", IsDefault = true },
                new Category { CategoryName = "Reversal", IsDefault = true }
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
