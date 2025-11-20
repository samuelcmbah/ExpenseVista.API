using ExpenseVista.API.Configurations;
using ExpenseVista.API.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace ExpenseVista.API.Services
{
    public class PaystackService: IPaystackService
    {
        private readonly HttpClient httpClient;
        private readonly string secretKey;

        public PaystackService(IOptions<PaystackSettings> options, IHttpClientFactory httpClientFactory)
        {
            secretKey = options.Value.SecretKey;
            httpClient = httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri("https://api.paystack.co/");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", secretKey);

        }

        // Initialize payment (returns authorization_url and reference)
        public async Task<(string authorizationUrl, string reference)> InitializePaymentAsync(string email, decimal amountNaira, string? callbackUrl = null, IDictionary<string, string>? metadata = null)
        {
            //assembles the request body as json payload
            var body = new Dictionary<string, object>
            {
                ["email"] = email,
                ["amount"] = (int)(amountNaira * 100) // kobo
            };
            if (!string.IsNullOrEmpty(callbackUrl)) body["callback_url"] = callbackUrl;
            if (metadata != null) body["metadata"] = metadata;

            //sends the request to paystack api endpoint
            var resp = await httpClient.PostAsync("transaction/initialize", new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"));
            var result = await resp.Content.ReadAsStringAsync();
            resp.EnsureSuccessStatusCode();//throws exception if failed
            //extracts the authurl and reference
            var doc = JsonDocument.Parse(result);
            var data = doc.RootElement.GetProperty("data");
            var authUrl = data.GetProperty("authorization_url").GetString()!;
            var reference = data.GetProperty("reference").GetString()!;
            return (authUrl, reference);
        }

        //Verify payment by reference
        public async Task<JsonElement> VerifyPaymentAsync(string reference)
        {
            var response = await httpClient.GetAsync($"transaction/verify/{reference}");
            var jsonresult = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to verify payment: {jsonresult}");
            }
            using var document = JsonDocument.Parse(jsonresult);
            return document.RootElement;
                
             
        }

        //Create transfer recipient
        public async Task<JsonElement> CreateTransferRecipientAsync(string name, string accountNumber, string bankCode, string currency = "NGN")
        {
            var body = new
            {
                type = "nuban",
                name = name,
                accountNumber = accountNumber,
                bankCode = bankCode,
                currency = currency
            };

            var response = await httpClient.PostAsync("transferrecipient", new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"));
            var jsonresult = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();
            return JsonDocument.Parse(jsonresult).RootElement;
        }

        // Initiate transfer
        public async Task<JsonElement> InitiateTransferAsync(string recipientCode, decimal amountNaira, string reason)
        {
            var body = new
            {
                source = "balance",
                amount = (int)(amountNaira * 100), // kobo
                recipient = recipientCode,
                reason
            };
            var response = await httpClient.PostAsync("transfer", new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"));
            var jsonResult = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();
            return JsonDocument.Parse(jsonResult).RootElement;
        }

        // Helper to verify Paystack webhook signature (HMAC SHA512 of raw payload using secret key)
        public bool VerifyWebhookSignature(string rawBody, string signatureHeader)
        {
            //Webhook signature verification uses HMAC-SHA512 (Paystack uses SHA512). We compute hex lowercase to compare.
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            using var hmac = new HMACSHA512(keyBytes);
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawBody));
            var computed = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            // Paystack header is hex lowercase
            return computed == signatureHeader?.ToLowerInvariant();
        }

    }
}
