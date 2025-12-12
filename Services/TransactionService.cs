using AutoMapper;
using AutoMapper.QueryableExtensions;
using ExpenseVista.API.Configurations;
using ExpenseVista.API.Data;
using ExpenseVista.API.DTOs.Pagination;
using ExpenseVista.API.DTOs.Transaction;
using ExpenseVista.API.Models;
using ExpenseVista.API.Services.IServices;
using ExpenseVista.API.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ExpenseVista.API.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IExchangeRateService exchangeRateService;
        private readonly ILogger<TransactionService> logger;
        private readonly string baseCurrrency;

        public TransactionService(ApplicationDbContext context, IMapper mapper, 
            IHttpContextAccessor httpContextAccessor, IExchangeRateService exchangeRateService,
            IOptions<AppSettings> appSettings, ILogger<TransactionService> logger)
        {
            this.context = context;
            this.mapper = mapper;
            this.httpContextAccessor = httpContextAccessor;
            this.exchangeRateService = exchangeRateService;
            this.logger = logger;
            baseCurrrency = appSettings.Value.BaseCurrency;
        }

        /// <summary>
        /// Retrieves a Transaction entity only if it belongs to the specified user.
        /// Throws KeyNotFoundException if the transaction is not found or unauthorized.
        /// </summary>
        private async Task<Transaction> GetTransactionEntityForUserAsync(int transactionId, string userId)
        {
            var transaction = await context.Transactions
                .Include(t=> t.Category)//eager loading
                .FirstOrDefaultAsync(c => c.Id == transactionId && c.ApplicationUserId == userId);

            if (transaction == null)
            {
                throw new KeyNotFoundException($"Transaction with ID {transactionId} not found or unauthorized.");
            }
            return transaction;
        }

        // --- PUBLIC SERVICE METHODS ---

        public async Task<PagedResponse<TransactionDTO>> GetAllAsync(string userId, FilterPagedTransactionDTO filterDTO)
        {
            var queryable = context.Transactions
                 .Include(t => t.Category)//eager loading
                 .Where(t => t.ApplicationUserId == userId)
                 .AsQueryable();

            // Description or Category search
            if (!string.IsNullOrWhiteSpace(filterDTO.SearchTerm))
            {
                var term = filterDTO.SearchTerm.ToLower();
                queryable = queryable.Where(t =>
                    t.Description!.ToLower().Contains(term) ||
                    t.Category.CategoryName.ToLower().Contains(term));
            }

            // Category filter
            if (!string.IsNullOrWhiteSpace(filterDTO.CategoryName))
            {
                queryable = queryable.Where(t => t.Category.CategoryName == filterDTO.CategoryName);
            }

            // Transaction type filter
            if (filterDTO.Type.HasValue)
            {
                queryable = queryable.Where(t => ((int)t.Type) == filterDTO.Type.Value);
            }

            // Date range filter
            if (filterDTO.StartDate.HasValue)
            {
                queryable = queryable.Where(t => t.TransactionDate >= filterDTO.StartDate.Value);
            }

            if (filterDTO.EndDate.HasValue)
            {
                queryable = queryable.Where(t => t.TransactionDate <= filterDTO.EndDate.Value);
            }

            //Total count (before pagination)
            var totalCount = await queryable.CountAsync();

            //Apply pagination
            var transactionList = await queryable
                .OrderByDescending(t => t.TransactionDate)// show recent transactions first
                .Paginate(filterDTO)
                .ProjectTo<TransactionDTO>(mapper.ConfigurationProvider)
                .AsNoTracking()// Performance improvement for read-only query
                .ToListAsync();

            return new PagedResponse<TransactionDTO>(
                transactionList,
                filterDTO.Page,
                filterDTO.RecordsPerPage,
                totalCount
            );

            
        }

        public async Task<TransactionDTO> GetByIdAsync(int id, string userId)
        {
            var transaction = await GetTransactionEntityForUserAsync(id, userId);

            return mapper.Map<TransactionDTO>(transaction)!;
        }

        public async Task<TransactionDTO> CreateAsync(TransactionCreateDTO transactionCreateDTO, string userId)
        {
            //validate category
            var category = await context.Categories
                .FirstOrDefaultAsync(c => 
                c.Id == transactionCreateDTO.CategoryId &&
                (c.ApplicationUserId == userId || string.IsNullOrEmpty(c.ApplicationUserId)));

            if (category == null)
                throw new InvalidOperationException("Invalid category or unauthorized access.");
            
            decimal rate;

            // Check if the transaction is already in the base currency
            if (transactionCreateDTO.Currency.Equals("NGN", StringComparison.OrdinalIgnoreCase))
            {
                rate = 1m;
            }
            else
            {
                // For all other currencies, fetch the rate using the resilient try/catch/fallback logic
                try
                {
                    // Attempt to get the live exchange rate
                    rate = await exchangeRateService.GetRateAsync(transactionCreateDTO.Currency, baseCurrrency);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Live exchange rate API failed for currency {Currency}. Falling back to cached rate.", transactionCreateDTO.Currency);

                    // Fallback to a cached rate
                    var cachedRate = await exchangeRateService.GetCachedRateAsync(transactionCreateDTO.Currency, baseCurrrency);

                    if (cachedRate.HasValue)
                    {
                        rate = cachedRate.Value;
                    }
                    else
                    {
                        // Critical failure: The API is down AND we have no cached rate to fall back on.
                        throw new InvalidOperationException($"Could not determine the exchange rate for {transactionCreateDTO.Currency}. Please try again later.");
                    }
                }
            }

            var transaction = mapper.Map<Transaction>(transactionCreateDTO)!;
            transaction.ApplicationUserId = userId;
            transaction.ExchangeRate = rate;
            transaction.ConvertedAmount = transactionCreateDTO.Amount * rate;
            
            context.Transactions.Add(transaction);
            await context.SaveChangesAsync();

            // to reload the 'Category' navigation property here 
            // to map the full TransactionDTO for the return value.(explicit loading)
            await context.Entry(transaction).Reference(t => t.Category).LoadAsync();

            return mapper.Map<TransactionDTO>(transaction)!;
        }


        public async Task UpdateAsync(int id, TransactionUpdateDTO transactionUpdateDTO, string userId)
        {
            var transaction = await GetTransactionEntityForUserAsync(id, userId);


            bool currencyChanged = transaction.Currency != transactionUpdateDTO.Currency;
            bool amountChanged = transaction.Amount != transactionUpdateDTO.Amount;

            if (currencyChanged || amountChanged)
            {
                var rate = await exchangeRateService.GetRateAsync(transactionUpdateDTO.Currency, baseCurrrency);
                transaction.Currency = transactionUpdateDTO.Currency;     // original currency
                transaction.Amount = transactionUpdateDTO.Amount;         // original amount
                transaction.ExchangeRate = rate;         // updated rate
                transaction.ConvertedAmount = transactionUpdateDTO.Amount * rate; // recalc NGN
            }

            // Map remaining simple fields (no currency logic here)
            transaction.Type = transactionUpdateDTO.Type;
            transaction.TransactionDate = transactionUpdateDTO.TransactionDate;
            transaction.CategoryId = transactionUpdateDTO.CategoryId;
            transaction.Description = transactionUpdateDTO.Description;

            await context.SaveChangesAsync();
            // Note: No return value needed for a successful void update
        }

        public async Task DeleteAsync(int id, string userId)
        {
            var transaction = await GetTransactionEntityForUserAsync(id, userId);

            context.Transactions.Remove(transaction);
            await context.SaveChangesAsync();
            // Note: No return value needed for a successful void delete
        }

        /// <summary>
        /// If you find that many services only need the Id, Amount, and Type of a transaction (e.g., for reporting sums)
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>returns a list of transactions with minimal properties</returns>
        public async Task<IEnumerable<TransactionLiteDTO>> GetAllLiteAsync(string userId)
        {
            var liteTransactions = await context.Transactions
                .Where(t => t.ApplicationUserId == userId)
                .AsNoTracking()
                .Select(t => new TransactionLiteDTO
                {
                    Id = t.Id,
                    Amount = t.ConvertedAmount,
                    Type = t.Type,
                    TransactionDate = t.TransactionDate
                })
                .ToListAsync();

            return liteTransactions;
        }

    }
}
