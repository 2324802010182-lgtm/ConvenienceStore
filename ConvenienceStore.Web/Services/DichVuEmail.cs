using System.Net;
using System.Net.Mail;

using ConvenienceStore.Web.ViewModels;
using Microsoft.Extensions.Options;

namespace ConvenienceStore.Web.Services
{
    public class DichVuEmail : IDichVuEmail
    {
        private readonly EmailSettings _emailSettings;

        public DichVuEmail(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task GuiEmailAsync(string emailNhan, string tieuDe, string noiDungHtml)
        {
            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.Email, _emailSettings.DisplayName),
                Subject = tieuDe,
                Body = noiDungHtml,
                IsBodyHtml = true
            };

            mailMessage.To.Add(emailNhan);

            using var smtpClient = new SmtpClient(_emailSettings.Host, _emailSettings.Port)
            {
                Credentials = new NetworkCredential(_emailSettings.Email, _emailSettings.Password),
                EnableSsl = true
            };

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}