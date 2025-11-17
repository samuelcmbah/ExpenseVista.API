using ExpenseVista.API.Data;
using ExpenseVista.API.DTOs.Analytics;
using ExpenseVista.API.DTOs.Transaction;
using ExpenseVista.API.Models.Enums;
using ExpenseVista.API.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace ExpenseVista.API.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly ApplicationDbContext context;
        private readonly IPeriodicSummaryService periodicSummaryService;

        public AnalyticsService(ApplicationDbContext context, IPeriodicSummaryService periodicSummaryService)
        {
            this.context = context;
            this.periodicSummaryService = periodicSummaryService;
        }


        private async Task<BudgetProgressDTO> GetBudgetProgressAsync(string userId, DateTime startDate, DateTime endDate, decimal totalExpenses)
        {
            var budgets = await context.Budgets
                .Where(b => 
                    b.ApplicationUserId == userId && 
                    b.BudgetMonth >= startDate &&
                    b.BudgetMonth < endDate)
                .ToListAsync();

            decimal totalBudget = budgets.Sum(b => b.MonthlyLimit);
            decimal percentage = totalBudget > 0
                ? Math.Round((totalExpenses / totalBudget) * 100, 2)
                : 0;

            return new BudgetProgressDTO
            {
                Spent = totalExpenses,
                Total = totalBudget,
                Percentage = percentage
            };
        }

        private FinancialTransactionAnalytics GetTransactionAnalytics(List<TransactionDTO> transactions, decimal totalIncome, decimal totalExpenses)
        {
           
            var categorySpending = transactions
                .Where(t => t.Type == TransactionType.Expense)
                .GroupBy(t => t.Category.CategoryName)
                .Select(g => new SpendingCategoryDTO
                {
                    Name = g.Key,
                    Value = g.Sum(x => x.Amount),
                    Percentage = totalExpenses > 0
                        ? Math.Round((g.Sum(x => x.Amount) / totalExpenses) * 100, 2)
                        : 0
                })
                .ToList();

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

            var topCategory = categorySpending
                .OrderByDescending(c => c.Value)
                .FirstOrDefault();
            var netBalance = totalIncome - totalExpenses;
            var savingsRate = totalIncome > 0 ? (netBalance / totalIncome) * 100 : 0;

            return new FinancialTransactionAnalytics
            {
                Summary = new SummaryDTO
                {
                    TotalIncome = totalIncome,
                    TotalExpenses = totalExpenses,
                    NetBalance = netBalance,
                    SavingsRate = savingsRate
                },
                SpendingByCategory = categorySpending,
                IncomeVsExpenses = incomeVsExpenses,
                FinancialTrend = incomeVsExpenses,
                KeyInsights = new KeyInsightsDTO
                {
                    TopSpendingCategory = topCategory?.Name ?? "N/A",
                    TopSpendingAmount = topCategory?.Value ?? 0,
                    TotalTransactions = transactions.Count,
                    TotalIncomeTransactions = transactions.Count(t => t.Type == TransactionType.Income),
                    TotalExpenseTransactions = transactions.Count(t => t.Type == TransactionType.Expense)
                }
            };
        }


        public async Task<FinancialDataDTO> GetAnalyticsAsync(string period, string userId)
        {
            var summary = await periodicSummaryService.GetPeriodicSummaryAsync(userId, period);

            var budgetProgress = await GetBudgetProgressAsync(userId, summary.StartDate, summary.EndDate, summary.TotalExpenses);
            if (summary.Transactions == null)
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

            var analytics = GetTransactionAnalytics(summary.Transactions, summary.TotalIncome, summary.TotalExpenses);

            return new FinancialDataDTO
            {
                TimePeriod = period,
                Summary = analytics.Summary,
                BudgetProgress = budgetProgress,
                SpendingByCategory = analytics.SpendingByCategory,
                IncomeVsExpenses = analytics.IncomeVsExpenses,
                FinancialTrend = analytics.FinancialTrend,
                keyInsights = analytics.KeyInsights
            };
        }
    }
}
