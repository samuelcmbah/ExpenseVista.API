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
        private readonly IPeriodicSummaryService periodicSummaryService;

        public BudgetService(ApplicationDbContext context, IMapper mapper, ITransactionService transactionService, IPeriodicSummaryService periodicSummaryService)
        {
            this.context = context;
            this.mapper = mapper;
            this.transactionService = transactionService;
            this.periodicSummaryService = periodicSummaryService;
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

        public async Task<BudgetStatusDTO> GetBudgetStatusForMonthAsync(string userId)
        {
            var summary = await periodicSummaryService.GetPeriodicSummaryAsync(userId);

            var budget = await context.Budgets
                .AsNoTracking()
                .FirstOrDefaultAsync(b =>
                    b.ApplicationUserId == userId &&
                    b.BudgetMonth >= summary.StartDate &&
                    b.BudgetMonth < summary.EndDate
                );

            if (budget == null)
            {
                return new BudgetStatusDTO
                {
                    BudgetSet = false,
                    BudgetMonth = summary.StartDate,
                    MonthlyLimit = 0,
                    TotalIncome = 0,
                    CurrentUsage = 0,
                    RemainingAmount = 0,
                    PercentageUsed = 0
                };
            }

            return new BudgetStatusDTO
            {
                Id = budget.Id,
                BudgetSet = true,
                BudgetMonth = budget.BudgetMonth,
                MonthlyLimit = budget.MonthlyLimit,
                RemainingAmount = Math.Max(budget.MonthlyLimit - summary.TotalExpenses, 0),
                PercentageUsed = budget.MonthlyLimit > 0
                    ? Math.Round((summary.TotalExpenses / budget.MonthlyLimit) * 100, 2)
                    : 0
            };
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
