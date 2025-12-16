
using AutoMapper;
using ExpenseVista.API.Data;
using ExpenseVista.API.DTOs.Analytics;
using ExpenseVista.API.DTOs.Transaction;
using ExpenseVista.API.Models.Enums;
using ExpenseVista.API.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace ExpenseVista.API.Services.Analytics
{
    public class PeriodicSummaryService : IPeriodicSummaryService
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public PeriodicSummaryService(ApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }
        public async Task<PeriodicSummaryDTO> GetPeriodicSummaryAsync(string userId, string period = "This Month")
        {
            DateTime now = DateTime.UtcNow;

            DateTime startDate;
            DateTime endDate = now;

            switch (period)
            {
                case "This Month":
                    startDate = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                    break;

                case "Last Month":
                    startDate = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-1);
                    endDate = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                    break;

                case "Last 3 Months":
                    // First, get the date 3 months ago
                    var roughStartDate3M = now.AddMonths(-3);
                    // Then, anchor it to the first day of that month
                    startDate = new DateTime(roughStartDate3M.Year, roughStartDate3M.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                    break;

                case "Last 6 Months":
                    // First, get the date 6 months ago
                    var roughStartDate6M = now.AddMonths(-6);
                    // Then, anchor it to the first day of that month
                    startDate = new DateTime(roughStartDate6M.Year, roughStartDate6M.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                    break;

                case "This Year":
                    startDate = new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    break;

                case "Last Year":
                    startDate = new DateTime(now.Year - 1, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    endDate = new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    break;

                default:
                    // Defaulting to "Last Month" is safer and more consistent
                    startDate = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-1);
                    endDate = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                    break;
            }


            var transactions = await context.Transactions
                .Include(t => t.Category)
                .Where(t =>
                    t.ApplicationUserId == userId &&
                    t.TransactionDate >= startDate &&
                    t.TransactionDate < endDate)
                .ToListAsync();

            decimal income = transactions
                .Where(t => t.Type == TransactionType.Income)
                .Sum(t => t.ConvertedAmount);

            decimal expenses = transactions
                .Where(t => t.Type == TransactionType.Expense)
                .Sum(t => t.ConvertedAmount);

            return new PeriodicSummaryDTO
            {
                StartDate = startDate,
                EndDate = endDate,
                Transactions = mapper.Map<List<TransactionDTO>>(transactions),
                TotalIncome = income,
                TotalExpenses = expenses
            };
        }
    }
}
