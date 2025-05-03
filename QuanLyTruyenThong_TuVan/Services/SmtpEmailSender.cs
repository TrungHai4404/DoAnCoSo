// File: Services/SmtpEmailSender.cs
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace QuanLyTruyenThong_TuVan.Services
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly EmailSettings _settings;
        public SmtpEmailSender(IOptions<EmailSettings> opts)
        {
            _settings = opts.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            using var msg = new MailMessage();
            msg.From = new MailAddress(_settings.From);
            msg.To.Add(email);
            msg.Subject = subject;
            msg.Body = htmlMessage;
            msg.IsBodyHtml = true;

            using var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                Credentials = new NetworkCredential(_settings.Username, _settings.Password),
                EnableSsl = _settings.EnableSsl
            };

            // SmtpClient.SendAsync có callback, nhưng dễ nhất là SendMailAsync:
            await client.SendMailAsync(msg);
        }
    }
}
