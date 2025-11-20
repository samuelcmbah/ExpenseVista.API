namespace ExpenseVista.API.DTOs.Payment
{
    public class InitializeTopUpDto
    {
        public string Email { get; set; } = string.Empty;
        public decimal Amount { get; set; } // in Naira
        public string? CallbackUrl { get; set; }
        public string? UserId { get; set; } // optional fallback
    }
}
