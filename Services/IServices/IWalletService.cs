using ExpenseVista.API.Models;

namespace ExpenseVista.API.Services.IServices
{
    public interface IWalletService
    {
        Task<Wallet> GetOrCreateWalletAsync(string userId);
        Task<WalletTransaction> CreditAsync(string userId, decimal amount, string source, string reference, string? description = null);
        Task<(bool success, WalletTransaction? transaction)> DebitAsync(string userId, decimal amount, string source, string reference, string? description = null);

    }
}
