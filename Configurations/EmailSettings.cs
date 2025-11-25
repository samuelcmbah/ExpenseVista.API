namespace ExpenseVista.API.Configurations
{
    public class EmailSettings
    {
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587;  // STARTTLS port
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool UseSsl { get; set; } = false; // false because we use STARTTLS
        public bool UseStartTls { get; set; } = true; // must enable STARTTLS
    }

}
