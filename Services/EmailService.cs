using ExpenseVista.API.Configurations;
using ExpenseVista.API.Services.IServices;
using Microsoft.Extensions.Options;
using MimeKit;
using Org.BouncyCastle.Ocsp;
using Resend;
using ILogger = Microsoft.Extensions.Logging.ILogger<EmailService>;

public class EmailService : IEmailService
{
    private readonly ILogger _logger;
    private readonly ResendEmailSettings _emailSettings;
    private readonly IResend _resend; // inject interface provided by SDK

    public EmailService(ILogger logger, IOptions<ResendEmailSettings> options, IResend resend)
    {
        _logger = logger;
        _emailSettings = options.Value;
        _resend = resend; // already configured with API key by DI
    }

    public async Task SendEmailAsync(string to, string verifyUrl)
    {
        var html = $@"
            <p>Thank you for signing up on ExpenseVista.</p>
            <p>Click the button below to verify your email address.</p>
            <div style='margin-top:20px;'>
                <a href='{verifyUrl}'
                   style='display:inline-block;padding:10px 20px;font-size:16px;
                   color:#fff;background-color:#28a745;text-decoration:none;
                   border-radius:5px;'>Verify Email</a>
            </div>
            <p>If you didn't sign up, please ignore this email.</p>";

        try
        {
            _logger.LogWarning($"Sending verification email to {to}");

            await _resend.EmailSendAsync(new EmailMessage
            {
                From = $"ExpenseVista <{_emailSettings.FromEmail}>",
                To = to,
                Subject = "Verify your ExpenseVista email",
                HtmlBody = html
            });

            _logger.LogWarning($"Verification email sent to {to}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error sending email: {ex.Message}");
            throw;
        }
    }

    public async Task SendPasswordResetEmailAsync(string to, string resetUrl)
    {
        var html = $@"
            <p>You requested to reset your password.</p>
            <div style='margin-top:20px;'>
                <a href='{resetUrl}'
                   style='display:inline-block;padding:10px 20px;font-size:16px;
                   color:#fff;background-color:#28a745;text-decoration:none;
                   border-radius:5px;'>Reset Password</a>
            </div>";

        try
        {
            await _resend.EmailSendAsync(new EmailMessage
            {
                From = $"ExpenseVista <{_emailSettings.FromEmail}>",
                To = to,
                Subject = "Reset your ExpenseVista password",
                HtmlBody = html
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error sending password reset email: {ex.Message}");
            throw;
        }
    }

    public async Task SendPasswordChangedNotification(string to)
    {
        var html = @"<p>Your ExpenseVista account password was changed.</p>
                     <p>If this wasn't you, reset your password immediately.</p>";

        try
        {
            await _resend.EmailSendAsync(new EmailMessage
            {
                From = $"ExpenseVista <{_emailSettings.FromEmail}>",
                To = to,
                Subject = "Your Password Was Changed",
                HtmlBody = html
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error sending password changed email: {ex.Message}");
            throw;
        }
    }
}
