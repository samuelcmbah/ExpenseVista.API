using ExpenseVista.API.DTOs.Category;
using ExpenseVista.API.DTOs.Pagination;
using ExpenseVista.API.DTOs.Transaction;

namespace ExpenseVista.API.Services.IServices
{
    public interface ITransactionService
    {
        Task<PagedResponse<TransactionDTO>> GetAllAsync(string userId, FilterPagedTransactionDTO filterDTO);
        Task<TransactionDTO> GetByIdAsync(int id, string userId);
        Task<TransactionDTO> CreateAsync(TransactionCreateDTO transactionCreateDTO, string userId);
        Task UpdateAsync(int id, TransactionUpdateDTO transactionUpdateDTO, string userId);
        Task DeleteAsync(int id, string userId);
        Task<IEnumerable<TransactionLiteDTO>> GetAllLiteAsync(string userId);
        Task CreateFromWalletAsync(string userId, decimal amount, string category, string type, string description, string source, string reference);

    }
}
