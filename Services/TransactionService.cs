using AutoMapper;
using AutoMapper.QueryableExtensions;
using ExpenseVista.API.Data;
using ExpenseVista.API.DTOs.Pagination;
using ExpenseVista.API.DTOs.Transaction;
using ExpenseVista.API.Models;
using ExpenseVista.API.Services.IServices;
using ExpenseVista.API.Utilities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseVista.API.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IHttpContextAccessor httpContextAccessor;

        public TransactionService(ApplicationDbContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            this.context = context;
            this.mapper = mapper;
            this.httpContextAccessor = httpContextAccessor;
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

        public async Task<PagedResponse<TransactionDTO>> GetAllAsync(string userId, PaginationDTO paginationDTO)
        {
            var queryable = context.Transactions
                .Where(t => t.ApplicationUserId == userId);

            var totalCount = await context.Transactions.CountAsync(t => t.ApplicationUserId == userId);

            var transactionList = await queryable
                .Include(t => t.Category)//eager loading
                .OrderByDescending(t => t.TransactionDate) // show recent transactions first
                .Paginate(paginationDTO)
                .AsNoTracking() // Performance improvement for read-only query
                .ProjectTo<TransactionDTO>(mapper.ConfigurationProvider)
                .ToListAsync();

            return new PagedResponse<TransactionDTO>(
                transactionList,
                paginationDTO.Page,
                paginationDTO.RecordsPerPage,
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
            var category = await context.Categories
                .FirstOrDefaultAsync(c => c.Id == transactionCreateDTO.CategoryId && c.ApplicationUserId == userId);
            if (category == null)
                throw new InvalidOperationException("Invalid category or unauthorized access.");

            var transaction = mapper.Map<Transaction>(transactionCreateDTO)!;
            transaction.ApplicationUserId = userId;

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

            mapper.Map(transactionUpdateDTO, transaction);
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
                    Amount = t.Amount,
                    Type = t.Type,
                    TransactionDate = t.TransactionDate
                })
                .ToListAsync();

            return liteTransactions;
        }

    }
}
