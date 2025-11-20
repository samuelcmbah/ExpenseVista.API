using System.Text.Json;

namespace ExpenseVista.API.Services.IServices
{
    public interface IPaystackService
    {
        Task<(string authorizationUrl, string reference)> InitializePaymentAsync(string email, decimal amountNaira, string? callbackUrl = null, IDictionary<string, string>? metadata = null);
        Task<JsonElement> VerifyPaymentAsync(string reference);
        Task<JsonElement> CreateTransferRecipientAsync(string name, string accountNumber, string bankCode, string currency = "NGN");
        Task<JsonElement> InitiateTransferAsync(string recipientCode, decimal amountNaira, string reason);
        bool VerifyWebhookSignature(string rawBody, string signatureHeader);
    }

}
