
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

            var income = transactions
                .Where(t => t.Type == TransactionType.Income)
                .Sum(t => t.Amount);

            var expenses = transactions
                .Where(t => t.Type == TransactionType.Expense)
                .Sum(t => t.Amount);

            return new PeriodicSummaryDTO
            {
                Period = startDate,
                Transactions = mapper.Map<List<TransactionDTO>>(transactions),
                TotalIncome = income,
                TotalExpenses = expenses
            };
        }
    }
}
