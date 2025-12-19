using ExpenseVista.API.Data;
using ExpenseVista.API.DTOs.Analytics;
using ExpenseVista.API.DTOs.Transaction;
using ExpenseVista.API.Models.Enums;
using ExpenseVista.API.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace ExpenseVista.API.Services.Analytics
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

        private FinancialTransactionAnalytics GetTransactionAnalytics(List<TransactionDTO> transactions, decimal totalIncome, decimal totalExpenses, decimal totalBudget)
        {
           
            var categorySpending = transactions
                .Where(t => t.Type == TransactionType.Expense)
                .GroupBy(t => t.Category.CategoryName)
                .Select(g => new SpendingCategoryDTO
                {
                    Name = g.Key,
                    Value = g.Sum(x => x.ConvertedAmount),
                    Percentage = totalExpenses > 0
                        ? g.Sum(x => x.ConvertedAmount) / totalExpenses
                        : 0
                })
                .ToList();

            var incomeVsExpenses = transactions
                .GroupBy(t => t.TransactionDate.ToString("MMM yyyy"))
                .Select(g => new IncomeExpenseDataDTO
                {
                    Month = g.Key,
                    Income = g.Where(x => x.Type == TransactionType.Income).Sum(x => x.ConvertedAmount),
                    Expenses = g.Where(x => x.Type == TransactionType.Expense).Sum(x => x.ConvertedAmount)
                })
                .OrderBy(x => DateTime.ParseExact(x.Month, "MMM yyyy", null))
                .ToList();

            var topCategory = categorySpending
                .OrderByDescending(c => c.Value)
                .FirstOrDefault();
            var netBalance = totalIncome - totalExpenses;
            var budgetBalance = totalBudget - totalExpenses;
           var overSpent = Math.Abs(totalBudget - totalExpenses);
            return new FinancialTransactionAnalytics
            {
                Summary = new SummaryDTO
                {
                    TotalIncome = totalIncome,
                    TotalExpenses = totalExpenses,
                    NetBalance = netBalance,
                    BudgetBalance = budgetBalance,
                    OverSpent = overSpent
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

        private async Task<List<MonthlyBudgetDetailDTO>> GetMonthlyBudgetDetailsAsync(string userId, DateTime startDate, DateTime endDate, List<TransactionDTO> transactions)
        {
            // 1. Get all relevant budgets in one call
            var budgets = await context.Budgets
                .Where(b => b.ApplicationUserId == userId && b.BudgetMonth >= startDate && b.BudgetMonth < endDate)
                .ToDictionaryAsync(b => b.BudgetMonth.ToString("MMM yyyy")); // Key by "Month Year" string

            // 2. Group expenses by month
            var monthlyExpenses = transactions
                .Where(t => t.Type == TransactionType.Expense)
                .GroupBy(t => t.TransactionDate.ToString("MMM yyyy"))
                .ToDictionary(g => g.Key, g => g.Sum(t => t.ConvertedAmount));

            // 3. Create a list of all unique months from both budgets and expenses
            var allMonths = budgets.Keys.Union(monthlyExpenses.Keys).Distinct();

            var budgetDetails = new List<MonthlyBudgetDetailDTO>();

            // 4. Build the detailed list
            foreach (var monthKey in allMonths.OrderBy(m => DateTime.ParseExact(m, "MMM yyyy", null)))
            {
                budgets.TryGetValue(monthKey, out var budget);
                monthlyExpenses.TryGetValue(monthKey, out var spent);

                budgetDetails.Add(new MonthlyBudgetDetailDTO
                {
                    Month = monthKey,
                    BudgetAmount = budget?.MonthlyLimit, // Null if no budget found
                    AmountSpent = spent
                });
            }

            return budgetDetails;
        }

        public async Task<FinancialDataDTO> GetAnalyticsAsync(string period, string userId)
        {
            var summary = await periodicSummaryService.GetPeriodicSummaryAsync(userId, period);

            var monthlyBudgets = await GetMonthlyBudgetDetailsAsync(userId, summary.StartDate, summary.EndDate, summary.Transactions);


            if (summary.Transactions == null || !summary.Transactions.Any())
            {
                return new FinancialDataDTO
                {
                    TimePeriod = period,
                    StartDate = summary.StartDate,
                    EndDate = summary.EndDate,
                    Summary = new SummaryDTO(),
                    BudgetProgress = new BudgetProgressDTO(),
                    SpendingByCategory = new List<SpendingCategoryDTO>(),
                    IncomeVsExpenses = new List<IncomeExpenseDataDTO>(),
                    FinancialTrend = new List<IncomeExpenseDataDTO>(),
                    keyInsights = new KeyInsightsDTO(),
                    Transactions = new List<TransactionDTO>(), /// for mapping to exports
                    MonthlyBudgets = monthlyBudgets
                };
            }

            decimal totalBudgetForPeriod = monthlyBudgets.Sum(b => b.BudgetAmount ?? 0);
            decimal totalExpensesForPeriod = summary.TotalExpenses;
            decimal percentage = totalBudgetForPeriod > 0
                ? totalExpensesForPeriod / totalBudgetForPeriod 
                : 0;

            var budgetProgress = new BudgetProgressDTO
            {
                Total = totalBudgetForPeriod,
                Spent = totalExpensesForPeriod,
                Percentage = percentage
            };

            var analytics = GetTransactionAnalytics(summary.Transactions, summary.TotalIncome, summary.TotalExpenses, totalBudgetForPeriod);

            return new FinancialDataDTO
            {
                TimePeriod = period,
                StartDate = summary.StartDate,
                EndDate = summary.EndDate,
                Summary = analytics.Summary,
                BudgetProgress = budgetProgress,
                SpendingByCategory = analytics.SpendingByCategory,
                IncomeVsExpenses = analytics.IncomeVsExpenses,
                FinancialTrend = analytics.FinancialTrend,
                keyInsights = analytics.KeyInsights,
                Transactions = summary.Transactions, // for mapping to exports
                MonthlyBudgets = monthlyBudgets //for mapping to exports
            };
        }

    }
}
