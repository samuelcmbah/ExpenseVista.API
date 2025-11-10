using ExpenseVista.API.Data;
using ExpenseVista.API.DTOs.Analytics;
using ExpenseVista.API.Models.Enums;
using ExpenseVista.API.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace ExpenseVista.API.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly ApplicationDbContext context;

        public AnalyticsService(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<FinancialDataDTO> GetAnalyticsAsync(string period, string userId)
        {
            // Determine date range
            DateTime startDate = period switch
            {
                "This Month" => new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1),
                "Last 3 Months" => DateTime.Now.AddMonths(-3),
                "Last 6 Months" => DateTime.Now.AddMonths(-6),
                "This Year" => new DateTime(DateTime.Now.Year, 1, 1),
                _ => DateTime.Now.AddMonths(-1)
            };

            //  Get transactions for user in that period
            var transactions = await context.Transactions
                .Include(t => t.Category)
                .Where(t => t.ApplicationUserId == userId && t.TransactionDate >= startDate)
                .ToListAsync();

            if (!transactions.Any())
            {
                return new FinancialDataDTO
                {
                    TimePeriod = period,
                    Summary = new SummaryDTO(),
                    BudgetProgress = new BudgetProgressDTO(),
                    SpendingByCategory = new List<SpendingCategoryDTO>(),
                    IncomeVsExpenses = new List<IncomeExpenseDataDTO>(),
                    FinancialTrend = new List<IncomeExpenseDataDTO>(),
                    keyInsights = new KeyInsightsDTO()
                };
            }

            //  Compute totals
            decimal totalIncome = transactions
                .Where(t => t.Type == TransactionType.Income)
                .Sum(t => t.Amount);

            decimal totalExpenses = transactions
                .Where(t => t.Type == TransactionType.Expense)
                .Sum(t => t.Amount);

            // Budget Progress (for this month)
            var budgets = await context.Budgets
                .Where(b => b.ApplicationUserId == userId && b.BudgetMonth >= startDate)
                .ToListAsync();
               

            decimal spent = totalExpenses;
            decimal totalBudget = budgets.Sum(b => b.MonthlyLimit);
            decimal percentage = totalBudget > 0
                ? Math.Round((spent / totalBudget) * 100, 2)
                : 0;

            // Spending by Category (pie data)
            var categorySpending = transactions
                .Where(t => t.Type == TransactionType.Expense)
                .GroupBy(t => t.Category.CategoryName)
                .Select(g => new SpendingCategoryDTO
                {
                    Name = g.Key,
                    Value = g.Sum(x => x.Amount),
                    Percentage = spent > 0
                        ? Math.Round((g.Sum(x => x.Amount) / spent) * 100, 2)
                        : 0
                })
                .ToList();

            // Income vs Expenses by Month
            var incomeVsExpenses = transactions
                .GroupBy(t => t.TransactionDate.ToString("MMM yyyy"))
                .Select(g => new IncomeExpenseDataDTO
                {
                    Month = g.Key,
                    Income = g.Where(x => x.Type == TransactionType.Income).Sum(x => x.Amount),
                    Expenses = g.Where(x => x.Type == TransactionType.Expense).Sum(x => x.Amount)
                })
                .OrderBy(x => DateTime.ParseExact(x.Month, "MMM yyyy", null))
                .ToList();

            // Key Insights
            var topCategory = categorySpending.OrderByDescending(c => c.Value).FirstOrDefault();

            var netBalance = totalIncome - totalExpenses;
            var savingsRate = totalIncome > 0 ? (netBalance / totalIncome) * 100 : 0;

            return new FinancialDataDTO
            {
                TimePeriod = period,
                Summary = new SummaryDTO
                {
                    TotalIncome = totalIncome,
                    TotalExpenses = totalExpenses,
                    NetBalance = netBalance,
                    SavingsRate = savingsRate
                },
                BudgetProgress = new BudgetProgressDTO
                {
                    Spent = spent,
                    Total = totalBudget,
                    Percentage = percentage
                },
                SpendingByCategory = categorySpending,
                IncomeVsExpenses = incomeVsExpenses,
                FinancialTrend = incomeVsExpenses, // same for now
                keyInsights = new KeyInsightsDTO
                {
                    TopSpendingCategory = topCategory?.Name ?? "N/A",
                    TopSpendingAmount = topCategory?.Value ?? 0,
                    TotalTransactions = transactions.Count,
                    TotalIncomeTransactions = transactions.Count(t => t.Type == TransactionType.Income),
                    TotalExpenseTransactions = transactions.Count(t => t.Type == TransactionType.Expense)
                }
            };
        }
    }
}
