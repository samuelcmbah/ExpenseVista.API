namespace ExpenseVista.API.Services.IServices
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string verifyUrl);
    }
}
