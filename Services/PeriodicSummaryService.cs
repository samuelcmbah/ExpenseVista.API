
using AutoMapper;
using ExpenseVista.API.Data;
using ExpenseVista.API.DTOs.Analytics;
using ExpenseVista.API.DTOs.Transaction;
using ExpenseVista.API.Models.Enums;
using ExpenseVista.API.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace ExpenseVista.API.Services
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
            DateTime now = DateTime.Now;

            DateTime startDate;
            DateTime endDate = now; // default end date is today unless period specifies otherwise

            switch (period)
            {
                case "This Month":
                    startDate = new DateTime(now.Year, now.Month, 1);
                    break;

                case "Last Month":
                    startDate = new DateTime(now.Year, now.Month, 1).AddMonths(-1); // 1st of last month
                    endDate = new DateTime(now.Year, now.Month, 1);                 // 1st of this month
                    break;

                case "Last 3 Months":
                    startDate = now.AddMonths(-3);
                    break;

                case "Last 6 Months":
                    startDate = now.AddMonths(-6);
                    break;

                case "This Year":
                    startDate = new DateTime(now.Year, 1, 1);
                    break;

                default:
                    startDate = now.AddMonths(-1);
                    break;
            }

            // Fetch range
            var transactions = await context.Transactions
                .Include(t => t.Category)
                .Where(t =>
                    t.ApplicationUserId == userId &&
                    t.TransactionDate >= startDate &&
                    t.TransactionDate < endDate)  // use < endDate for accurate month windows
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
