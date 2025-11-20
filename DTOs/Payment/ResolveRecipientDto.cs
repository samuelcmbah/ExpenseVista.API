namespace ExpenseVista.API.DTOs.Payment
{
    public class ResolveRecipientDto
    {
        public string Name { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string BankCode { get; set; } = string.Empty;
    }
}
