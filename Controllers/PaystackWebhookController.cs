using ExpenseVista.API.Services;
using ExpenseVista.API.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ExpenseVista.API.Controllers
{
    [ApiController]
    [Route("api/paystack")]
    public class PaystackWebhookController : ControllerBase
    {
        private readonly IPaystackService paystackService;
        private readonly IWalletService walletService;

        public PaystackWebhookController(IPaystackService paystackService, IWalletService walletService)
        {
            this.paystackService = paystackService;
            this.walletService = walletService;
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> Handle()
        {
            using var stream = new StreamReader(Request.Body);
            var raw = await stream.ReadToEndAsync();
            
            var signature = Request.Headers["x-paystack-signature"].FirstOrDefault();

            if (!paystackService.VerifyWebhookSignature(raw, signature!))
                return Unauthorized();

            var doc = JsonDocument.Parse(raw);
            var eventType = doc.RootElement.GetProperty("event").GetString();

            // For example: charge.success for payments
            if (eventType == "charge.success")
            {
                var data = doc.RootElement.GetProperty("data");
                var status = data.GetProperty("status").GetString();
                if (status == "success")
                {
                    var reference = data.GetProperty("reference").GetString();
                    var amount = data.GetProperty("amount").GetInt32(); // kobo
                    decimal amountNaira = amount / 100m;

                    // try to extract metadata.userId
                    string userId = "anonymous";
                    if (data.TryGetProperty("metadata", out var meta) && meta.ValueKind != JsonValueKind.Null)
                    {
                        if (meta.TryGetProperty("userId", out var uidEl)) userId = uidEl.GetString() ?? userId;
                    }

                    Console.WriteLine("I AM IN THE PAYSTACK WEBHOOK AND I AM LOGGING THIS MESSAGE TO THE CONSOLE TO SHOW THAT THE MESSAE GOT TO THE CONTROLLER SO THAT THE DEVELOPER CAN SEE IT");

                    // Credit user's wallet
                    await walletService.CreditAsync(userId, amountNaira, "Paystack", reference!, "Wallet Top-up (webhook)");
                }
            }
            // handle transfer.success or transfer.failed (update statuses)
            if (eventType == "transfer.success")
            {
                // optionally update transfer records or mark completed
            }

            return Ok();
        }
    }

}
