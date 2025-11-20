using ExpenseVista.API.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseVista.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/wallet")]
    public class WalletController : BaseController
    {
        private readonly IWalletService walletService;

        public WalletController(IWalletService wallet) => walletService = wallet;

        [HttpGet("balance")]
        public async Task<IActionResult> GetBalance()
        {
            var userId = GetUserId();
            var wallet = await walletService.GetOrCreateWalletAsync(userId);
            return Ok(new { balance = wallet.Balance });
        }

        [HttpGet("transactions")]
        public async Task<IActionResult> GetTransactions()
        {
            var userId = GetUserId();
            var wallet = await walletService.GetOrCreateWalletAsync(userId);
            var txs = wallet.Transactions.OrderByDescending(t => t.CreatedAt)
                .Select(t => new {
                    t.Id,
                    t.Amount,
                    t.Type,
                    t.Source,
                    t.Reference,
                    t.Description,
                    t.CreatedAt
                });
            return Ok(txs);
        }
    }

}
