namespace ExpenseVista.API.DTOs.Auth
{
    public class VerifyEmailDTO
    {
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}
