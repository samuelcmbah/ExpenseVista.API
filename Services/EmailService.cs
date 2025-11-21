using ExpenseVista.API.Configurations;
using ExpenseVista.API.Services.IServices;
using MailKit.Net.Smtp;
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
            HtmlBody = $"Click <a href='{verifyUrl}'>here</a> to verify your email."

        };
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        try
        {
            logger.LogInformation($"Attempting to send email to {to}");
            await client.ConnectAsync(_emailSettings.SmtpHost, _emailSettings.SmtpPort, _emailSettings.UseSsl);
            await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password); 
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            logger.LogInformation($"Email successfully sent to {to}");
        }
        catch (Exception ex)
        {
            logger.LogError($"Error sending email to {to}: {ex.Message}");
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
}
