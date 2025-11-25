using ExpenseVista.API.Configurations;
using ExpenseVista.API.Services.IServices;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

public class EmailService : IEmailService
{
    private readonly IConfiguration config;
    private readonly ILogger<EmailService> logger;
    private readonly EmailSettings _emailSettings;
    public EmailService(IConfiguration config, ILogger<EmailService> logger, IOptions<EmailSettings> emailSettingsOptions)
    {
        this.config = config;
        this.logger = logger;
        //.Value gets emailsettings instance
        _emailSettings = emailSettingsOptions.Value;
    }

    public async Task SendEmailAsync(string to, string verifyUrl)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("ExpenseVista", _emailSettings.Username));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = "Verify your ExpenseVista email";

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = $@"
            <p>Thank you for signing up on ExpenseVista.</p>
            <p>Click the button below to verify your email addres.</p>
            <div style='margin-top:20px;'>
                <a href='{verifyUrl}' 
                   style='display:inline-block;
                          padding:10px 20px;
                          font-size:16px;
                          color:#fff;
                          background-color:#28a745;
                          text-decoration:none;
                          border-radius:5px;'>
                    Verify Email
                </a>
            </div>
            <p>If you didn't sign up for ExpenseVista, please ignore this email.</p>"
        };

        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        try
        {
            logger.LogWarning ($"Attempting to send email to {to}");
            if (_emailSettings.UseStartTls)
            {
                await client.ConnectAsync(_emailSettings.SmtpHost, _emailSettings.SmtpPort, SecureSocketOptions.StartTls);
            }
            else
            {
                await client.ConnectAsync(_emailSettings.SmtpHost, _emailSettings.SmtpPort, _emailSettings.UseSsl);
            }
            await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password); 
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            logger.LogWarning($"Email successfully sent to {to}");
        }
        catch (Exception ex)
        {
            logger.LogError($"Error sending confirmation email to {to}: {ex.Message}");
            //depending on requirement
            throw;
        }
        finally
        {
            if (client.IsConnected)
            {
                await client.DisconnectAsync(true );
            }
        }
    }

    public async Task SendPasswordResetEmailAsync(string to, string resetUrl)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("ExpenseVista", _emailSettings.Username));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = "Reset your ExpenseVista password";

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = $@"
            <p>You requested to reset your password.</p>
            <p>If you didn’t request this, please ignore this email.</p>
            <div style='margin-top:20px;'>
                <a href='{resetUrl}' 
                   style='display:inline-block;
                          padding:10px 20px;
                          font-size:16px;
                          color:#fff;
                          background-color:#28a745;
                          text-decoration:none;
                          border-radius:5px;'>
                    Reset Password
                </a>
            </div>"
        };


        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        try
        {
            if (_emailSettings.UseStartTls)
            {
                await client.ConnectAsync(_emailSettings.SmtpHost, _emailSettings.SmtpPort, SecureSocketOptions.StartTls);
            }
            else
            {
                await client.ConnectAsync(_emailSettings.SmtpHost, _emailSettings.SmtpPort, _emailSettings.UseSsl);
            }
            await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
            await client.SendAsync(message);
        }
        catch (Exception ex)
        {
            logger.LogError($"Error sending reset email to {to}: {ex.Message}");
            //depending on requirement
            throw;
        }
        finally
        {
            if (client.IsConnected)
            {
                await client.DisconnectAsync(true);
            }
        }

    }

    public async Task SendPasswordChangedNotification(string to)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("ExpenseVista", _emailSettings.Username));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = "Your ExpenseVista Password Was Changed";

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = @"
            <p>Hello,</p>
            <p>This is a notification that your ExpenseVista account password was recently changed.</p>
            <p>If you did not perform this action, please reset your password immediately or contact support.</p>
            <p>Thank you,<br/>ExpenseVista Team</p>
        "
        };

        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        try
        {
            if (_emailSettings.UseStartTls)
            {
                await client.ConnectAsync(_emailSettings.SmtpHost, _emailSettings.SmtpPort, SecureSocketOptions.StartTls);
            }
            else
            {
                await client.ConnectAsync(_emailSettings.SmtpHost, _emailSettings.SmtpPort, _emailSettings.UseSsl);
            }
            await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
            await client.SendAsync(message);
        }
        finally
        {
            await client.DisconnectAsync(true);
        }
    }


}
