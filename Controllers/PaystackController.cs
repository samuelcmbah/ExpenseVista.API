using ExpenseVista.API.DTOs.Payment;
using ExpenseVista.API.Services;
using ExpenseVista.API.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseVista.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/paystack")]
    public class PaystackController : BaseController
    {
        private readonly IPaystackService paystackService;
        private readonly IWalletService walletService;

        public PaystackController(IPaystackService paystackService, IWalletService walletService)
        {
            this.paystackService = paystackService;
            this.walletService = walletService;
        }

        // Initialize top-up
        [HttpPost("initialize-topup")]
        public async Task<IActionResult> InitializeTopUp([FromBody] InitializeTopUpDto dto)
        {
            string userId = GetUserId();
            // include userId in metadata so we can map on verification/webhook
            var metadata = new Dictionary<string, string> { ["userId"] = userId };
            var callbackUrl = dto.CallbackUrl; // optional
            var (authUrl, reference) = await paystackService.InitializePaymentAsync(dto.Email, dto.Amount, callbackUrl, metadata);

            // Optionally persist a pending record to DB if you want
            return Ok(new { authorizationUrl = authUrl, reference });
        }

        // Verify after redirect (not required if you rely on webhooks)
        [HttpGet("verify-payment")]
        public async Task<IActionResult> Verify([FromQuery] string reference)
        {
            var result = await paystackService.VerifyPaymentAsync(reference);
            var status = result.GetProperty("data").GetProperty("status").GetString();

            if (status != "success")
            {
                return BadRequest(new { message = "Payment not successful", status });
            }

            var metadata = result.GetProperty("data").GetProperty("metadata");
            string userId = metadata.TryGetProperty("userId", out var v) ? v.GetString()! : GetUserId() ?? "anonymous";

            var amountKobo = result.GetProperty("data").GetProperty("amount").GetInt32();
            decimal amountNaira = amountKobo / 100m;
            string referenceFromPay = result.GetProperty("data").GetProperty("reference").GetString()!;

            await walletService.CreditAsync(userId, amountNaira, "Paystack", referenceFromPay, "Wallet Top-up");

            return Ok(new { message = "Wallet credited", amount = amountNaira });
        }
    }
}
