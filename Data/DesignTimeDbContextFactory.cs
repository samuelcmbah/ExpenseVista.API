using ExpenseVista.API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<ApplicationDbContext>();

        // Use local Postgres for migrations
        builder.UseNpgsql(
            "Host=localhost;Port=5432;Database=expensevista;Username=postgres;Password=postgres"
        );

        return new ApplicationDbContext(builder.Options);
    }
}
