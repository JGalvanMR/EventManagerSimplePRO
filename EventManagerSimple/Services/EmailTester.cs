using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace EventManager.Services
{
    public static class EmailTester
    {
        public static async Task<bool> TestConnection(EmailConfig config)
        {
            try
            {
                using (var client = new SmtpClient(config.SmtpServer, config.SmtpPort))
                {
                    client.Credentials = new NetworkCredential(config.Username, config.Password);
                    client.EnableSsl = config.UseSsl;
                    client.Timeout = 10000;

                    await client.SendMailAsync(
                        new MailMessage(config.SenderEmail, config.SenderEmail)
                        {
                            Subject = "Prueba de conexión",
                            Body = "Esta es una prueba de conexión SMTP exitosa.",
                            IsBodyHtml = false
                        }
                    );

                    return true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error de conexión SMTP: {ex.Message}", ex);
            }
        }
    }
}