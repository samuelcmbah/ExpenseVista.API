using ExpenseVista.API.DTOs.Analytics;
using ExpenseVista.API.DTOs.Analytics.Exports;

namespace ExpenseVista.API.Utilities
{
    public static class FinancialReportExportMapper
    {
        public static FinancialReportExport MapToExport(FinancialDataDTO source)
        {
            return new FinancialReportExport
            {
                TimePeriod = source.TimePeriod,

                Overview = new ReportOverviewExport
                {
                    TotalIncome = source.Summary.TotalIncome,
                    TotalExpenses = source.Summary.TotalExpenses,
                    NetBalance = source.Summary.NetBalance,

                    //BudgetTotal = source.BudgetProgress.Total,
                    //BudgetUsedPercentage = source.BudgetProgress.Percentage,
                    //BudgetBalance = source.Summary.BudgetBalance,

                    TopSpendingCategory = source.keyInsights.TopSpendingCategory,
                    TopSpendingAmount = source.keyInsights.TopSpendingAmount,

                    TotalTransactions = source.keyInsights.TotalTransactions,
                    IncomeTransactions = source.keyInsights.TotalIncomeTransactions,
                    ExpenseTransactions = source.keyInsights.TotalExpenseTransactions
                },

                CategorySpending = source.SpendingByCategory
                    .Select(c => new CategorySpendingExport
                    {
                        Category = c.Name,
                        AmountSpent = c.Value,
                        Percentage = c.Percentage
                    })
                    .ToList(),

                MonthlyIncomeVsExpenses = source.IncomeVsExpenses
                    .Select(m => new MonthlyIncomeExpenseExport
                    {
                        Month = m.Month,
                        Income = m.Income,
                        Expenses = m.Expenses
                    })
                    .ToList(),
                BudgetBreakdown = source.MonthlyBudgets
                .Select(mb => new MonthlyBudgetExport
                {
                    Month = mb.Month,
                    Budget = mb.BudgetAmount,
                    Spent = mb.AmountSpent,
                    Balance = mb.BudgetAmount.HasValue
                        ? mb.BudgetAmount.Value - mb.AmountSpent
                        : null
                })
                .ToList(),

                Transactions = source.Transactions
                .Select(t => new TransactionExport
                {
                    Date = t.TransactionDate,
                    Description = t.Description ?? string.Empty,
                    Category = t.Category.CategoryName,
                    // If it's an Income transaction, populate the Income property. Otherwise, it's null.
                    Income = t.Type == Models.Enums.TransactionType.Income
                        ? t.ConvertedAmount
                        : null,
                    Expense = t.Type == Models.Enums.TransactionType.Expense
                        ? t.ConvertedAmount
                        : null,
                })
                .OrderBy(t => t.Date) // Sort by oldest first
                .ToList()
            };
        }

    }
}
