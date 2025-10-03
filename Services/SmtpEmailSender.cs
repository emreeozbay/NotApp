using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;

namespace NotApp.Services
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpOptions _opt;
        public SmtpEmailSender(IOptions<SmtpOptions> opt) => _opt = opt.Value;

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            using var client = new SmtpClient(_opt.Host, _opt.Port)
            {
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,                 // <-- mutlaka false
                EnableSsl = _opt.EnableSsl,                    // 587 (STARTTLS) ya da 465 (SSL)
                Credentials = new NetworkCredential(_opt.User, _opt.Password),
                Timeout = 15000
            };

            var msg = new MailMessage
            {
                From = new MailAddress(_opt.From, _opt.FromName),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };
            msg.To.Add(email);

            try
            {
                await client.SendMailAsync(msg);
            }
            finally
            {
                msg.Dispose();
            }
        }
    }
}
