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

                    BudgetTotal = source.BudgetProgress.Total,
                    BudgetUsedPercentage = source.BudgetProgress.Percentage,
                    BudgetBalance = source.Summary.BudgetBalance,

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

                FinancialTrends = source.FinancialTrend
                    .Select(m => new MonthlyIncomeExpenseExport
                    {
                        Month = m.Month,
                        Income = m.Income,
                        Expenses = m.Expenses
                    })
                    .ToList()
            };
        }

    }
}
