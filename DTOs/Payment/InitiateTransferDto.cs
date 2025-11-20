namespace ExpenseVista.API.DTOs.Payment
{
    public class InitiateTransferDto
    {
        public string RecipientCode { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? Reason { get; set; }
        public string? UserId { get; set; }
    }
}
