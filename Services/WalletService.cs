using ExpenseVista.API.Data;
using ExpenseVista.API.Models;
using ExpenseVista.API.Services.IServices;
using Microsoft.EntityFrameworkCore;

public class WalletService : IWalletService
{
    private readonly ApplicationDbContext context;
    private readonly ITransactionService transactionService; // your existing service to create ExpenseVista transactions

    public WalletService(ApplicationDbContext context, ITransactionService transactionService)
    {
        this.context = context;
        this.transactionService = transactionService;
    }

    public async Task<Wallet> GetOrCreateWalletAsync(string userId)
    {
        var wallet = await context.Wallets.Include(w => w.Transactions).FirstOrDefaultAsync(w => w.ApplicationUserId == userId);
        if (wallet is null)
        {
            wallet = new Wallet { ApplicationUserId = userId, Balance = 0m };
            context.Wallets.Add(wallet);
            await context.SaveChangesAsync();
        }
        return wallet;
    }

    public async Task<WalletTransaction> CreditAsync(string userId, decimal amount, string source, string reference, string? description = null)
    {
        var wallet = await GetOrCreateWalletAsync(userId);
        wallet.Balance += amount;

        var wt = new WalletTransaction
        {
            WalletId = wallet.Id,
            Amount = amount,
            Type = "Credit",
            Source = source,
            Reference = reference,
            Description = description
        };

        context.WalletTransactions.Add(wt);
        await context.SaveChangesAsync();

        Console.WriteLine("wallet credit processed, creating ExpenseVista transaction...");

        // Create corresponding ExpenseVista transaction via your existing service
        // Example: Category "Funding", Type Income
        await transactionService.CreateFromWalletAsync(userId, amount, "Funding", "Income", description ?? "Wallet Top-up", source, reference);

        return wt;
    }

    public async Task<(bool success, WalletTransaction? transaction)> DebitAsync(string userId, decimal amount, string source, string reference, string? description = null)
    {
        var wallet = await GetOrCreateWalletAsync(userId);
        if (wallet.Balance < amount)
            return (false, null);

        wallet.Balance -= amount;

        var wt = new WalletTransaction
        {
            WalletId = wallet.Id,
            Amount = amount,
            Type = "Debit",
            Source = source,
            Reference = reference,
            Description = description
        };

        context.WalletTransactions.Add(wt);
        await context.SaveChangesAsync();

        await transactionService.CreateFromWalletAsync(userId, amount, "Transfer", "Expense", description ?? "Wallet Transfer", source, reference);

        return (true, wt);
    }
}
