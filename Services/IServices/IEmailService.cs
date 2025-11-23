namespace ExpenseVista.API.Services.IServices
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string verifyUrl);
        Task SendPasswordResetEmailAsync(string to, string resetUrl);
        Task SendPasswordChangedNotification(string to);

    }
}
