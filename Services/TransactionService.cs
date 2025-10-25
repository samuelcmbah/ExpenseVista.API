using AutoMapper;
using ExpenseVista.API.Data;
using ExpenseVista.API.DTOs.Transaction;
using ExpenseVista.API.Models;
using ExpenseVista.API.Services.IServices;
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

        public TransactionService(ApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
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

        public async Task<IEnumerable<TransactionDTO>> GetAllAsync(string userId) 
        {
            var transactions = await context.Transactions 
                .Where(t => t.ApplicationUserId == userId)
                .Include(t => t.Category)//eager loading
                .AsNoTracking() // Performance improvement for read-only query
                .ToListAsync();

            return mapper.Map<IEnumerable<TransactionDTO>>(transactions)!;
        }

        public async Task<TransactionDTO> GetByIdAsync(int id, string userId)
        {
            var transaction = await GetTransactionEntityForUserAsync(id, userId);

            return mapper.Map<TransactionDTO>(transaction)!;
        }

        public async Task<TransactionDTO> CreateAsync(TransactionCreateDTO transactionCreateDTO, string userId)
        {
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
    }
}
