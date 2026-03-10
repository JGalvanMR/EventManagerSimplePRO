using System;

namespace EventManager.Services
{
    public static class SMTPConfigurations
    {
        public static EmailConfig GmailConfig(string email, string password)
        {
            return new EmailConfig
            {
                SmtpServer = "smtp.gmail.com",
                SmtpPort = 587,
                UseSsl = true,
                SenderEmail = email,
                SenderName = "Evento Empresarial",
                Username = email,
                Password = password
            };
        }

        public static EmailConfig OutlookConfig(string email, string password)
        {
            return new EmailConfig
            {
                SmtpServer = "smtp-mail.outlook.com",
                SmtpPort = 587,
                UseSsl = true,
                SenderEmail = email,
                SenderName = "Evento Empresarial",
                Username = email,
                Password = password
            };
        }

        // ... otras configuraciones
    }
}