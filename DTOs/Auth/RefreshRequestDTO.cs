namespace ExpenseVista.API.DTOs.Auth
{
    public class RefreshRequestDTO
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
    }
}
