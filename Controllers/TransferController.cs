using ExpenseVista.API.DTOs.Payment;
using ExpenseVista.API.Services;
using ExpenseVista.API.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseVista.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/transfer")]
    public class TransferController : BaseController
    {
        private readonly IPaystackService paystackService;
        private readonly IWalletService walletservice;

        public TransferController(IPaystackService paystackService, IWalletService walletservice)
        {
            this.paystackService = paystackService;
            this.walletservice = walletservice;
        }

        // Resolve & create recipient
        [HttpPost("resolve")]
        public async Task<IActionResult> CreateRecipient([FromBody] ResolveRecipientDto dto)
        {
            // You could call /bank/resolve first to confirm, but Paystack's transferrecipient accepts the details
            var resp = await paystackService.CreateTransferRecipientAsync(dto.Name, dto.AccountNumber, dto.BankCode);
            var data = resp.GetProperty("data");
            return Ok(new
            {
                recipient_code = data.GetProperty("recipient_code").GetString(),
                name = data.GetProperty("name").GetString()
            });
        }

        // Initiate transfer (debit wallet then call paystack transfer)
        [HttpPost("initiate")]
        public async Task<IActionResult> InitiateTransfer([FromBody] InitiateTransferDto dto)
        {
            var userId = GetUserId() ?? dto.UserId ?? "anonymous";

            // Debit wallet first
            var (success, transaction) = await walletservice.DebitAsync(userId, dto.Amount, "TransferInit", Guid.NewGuid().ToString(), $"Transfer to {dto.RecipientCode}");
            if (!success)
            {
                return BadRequest(new { message = "Insufficient wallet balance" });
            }

            // Call Paystack transfer
            var paystackResp = await paystackService.InitiateTransferAsync(dto.RecipientCode, dto.Amount, dto.Reason ?? "ExpenseVista transfer");
            var data = paystackResp.GetProperty("data");
            var transferId = data.GetProperty("id").GetInt32();
            var status = data.GetProperty("status").GetString();

            // Optionally record Paystack transfer id into WalletTransaction.Reference or another table
            // update transaction.Reference = $"ps_transfer_{transferId}"; await _db.SaveChangesAsync() - left as exercise

            return Ok(new { transfer_id = transferId, status });
        }
    }
}
