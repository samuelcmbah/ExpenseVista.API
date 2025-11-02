using AutoMapper;
using ExpenseVista.API.Data;
using ExpenseVista.API.DTOs.Budget;
using ExpenseVista.API.Models;
using ExpenseVista.API.Models.Enums;
using ExpenseVista.API.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace ExpenseVista.API.Services
{
    public class BudgetService : IBudgetService
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly ITransactionService transactionService;

        public BudgetService(ApplicationDbContext context, IMapper mapper, ITransactionService transactionService)
        {
            this.context = context;
            this.mapper = mapper;
            this.transactionService = transactionService;
        }

        private async Task<Budget> GetBudgetEntityForUserAsync(int budgetId, string userId)
        {
            var budget = await context.Budgets
                .FirstOrDefaultAsync(b => b.Id == budgetId && b.ApplicationUserId == userId);

            if (budget == null)
            {
                throw new KeyNotFoundException($"Budget with ID {budgetId} not found or unauthorized.");
            }
            return budget;
        }

        public async Task<BudgetStatusDTO> GetBudgetStatusForMonthAsync(DateTime month, string userId)
        {
            //identify month and get budget for month
            var targetMonthStart = new DateTime(month.Year, month.Month, 1);
            var nextMonthStart = targetMonthStart.AddMonths(1);
             
            var budget = await context.Budgets
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.ApplicationUserId == userId 
                        && b.BudgetMonth >= targetMonthStart 
                        && b.BudgetMonth < nextMonthStart);

            if (budget == null)
            {
                throw new KeyNotFoundException($"Budget not set for {targetMonthStart.ToString("MMMM yyyy")}.");
            }
            var startDate = targetMonthStart;
            var endDate = targetMonthStart.AddMonths(1).AddDays(-1);

            // Filter the local collection to the specific period and calculate total expenses
            var transactions = await transactionService.GetAllLiteAsync(userId);
            var totalExpenses = transactions
                .Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate)
                .Where(t => t.Type == Models.Enums.TransactionType.Expense)
                .Sum(t => t.Amount);
            //or use this
            //var totalExpenses = await context.Transactions
            //    .Where(t => t.ApplicationUserId == userId &&
            //        t.TransactionDate >= startDate &&
            //        t.TransactionDate <= endDate &&
            //        t.Type == TransactionType.Expense)
            //    .SumAsync(t => (decimal?)t.Amount) ?? 0;

            var totalIncome = transactions.Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate)
                                          .Where(t => t.Type == Models.Enums.TransactionType.Income)
                                          .Sum(t => t.Amount);


            var budgetStatusDto = mapper.Map<BudgetStatusDTO>(budget)!;

            // Calculate status fields
            budgetStatusDto.CurrentUsage = totalExpenses;
            budgetStatusDto.TotalIncome = totalIncome;
            budgetStatusDto.RemainingAmount = budgetStatusDto.MonthlyLimit - budgetStatusDto.CurrentUsage;
            budgetStatusDto.PercentageUsed = (budget.MonthlyLimit > 0)
                ? Math.Round((totalExpenses / budget.MonthlyLimit) * 100, 2)
                : 0;

            return budgetStatusDto;
        }

        public async Task<BudgetDTO> CreateAsync(BudgetCreateDTO budgetCreateDTO, string userId)
        {
            var existingBudget = await context.Budgets
                .FirstOrDefaultAsync(b => b.ApplicationUserId == userId &&
                                            b.BudgetMonth.Month == budgetCreateDTO.BudgetMonth.Month &&
                                            b.BudgetMonth.Year == budgetCreateDTO.BudgetMonth.Year);

            if (existingBudget != null)
            {
                throw new InvalidOperationException("A budget already exists for the specified month.");
            }

            var budget = mapper.Map<Budget>(budgetCreateDTO)!;
            budget.ApplicationUserId = userId;

            await context.AddAsync(budget);
            await context.SaveChangesAsync();

            return mapper.Map<BudgetDTO>(budget)!;
        }

        public async Task<IEnumerable<BudgetDTO>> GetAllBudgetsAsync(string userId)
        {
            var userBudgets = await context.Budgets
                .Where(b => b.ApplicationUserId == userId)
                .OrderByDescending(b => b.BudgetMonth)
                .AsNoTracking()
                .ToListAsync();

            return mapper.Map<IEnumerable<BudgetDTO>>(userBudgets)!;
        }

        public async Task UpdateAsync(int id, BudgetUpdateDTO budgetUpdateDTO, string userId)
        {
            var budget = await GetBudgetEntityForUserAsync(id, userId);
            mapper.Map(budgetUpdateDTO, budget);

            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id, string userId)
        {
            var userBudget = await GetBudgetEntityForUserAsync(id, userId);
            context.Budgets.Remove(userBudget);
            await context.SaveChangesAsync();
        }
    }
}
