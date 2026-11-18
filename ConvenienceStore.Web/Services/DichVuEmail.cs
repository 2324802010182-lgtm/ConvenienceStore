using ConvenienceStore.Web.ViewModels;
using MailKit.Security;
using MimeKit;
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
            if (string.IsNullOrWhiteSpace(_emailSettings.SenderEmail) ||
                string.IsNullOrWhiteSpace(_emailSettings.Password) ||
                string.IsNullOrWhiteSpace(_emailSettings.Host) ||
                _emailSettings.Port <= 0)
            {
                throw new InvalidOperationException(
                    "Cấu hình Email chưa đầy đủ. Hãy thiết lập EmailSettings:Password bằng User Secrets hoặc biến môi trường.");
            }

            var mailMessage = new MimeMessage();
            mailMessage.From.Add(new MailboxAddress(
                _emailSettings.DisplayName,
                _emailSettings.SenderEmail));
            mailMessage.To.Add(MailboxAddress.Parse(emailNhan));
            mailMessage.Subject = tieuDe;
            mailMessage.Body = new BodyBuilder
            {
                HtmlBody = noiDungHtml,
                TextBody = "Vui lòng mở email bằng ứng dụng hỗ trợ HTML để xem đầy đủ nội dung."
            }.ToMessageBody();

            using var smtpClient = new MailKit.Net.Smtp.SmtpClient();
            await smtpClient.ConnectAsync(
                _emailSettings.Host,
                _emailSettings.Port,
                SecureSocketOptions.StartTls);
            await smtpClient.AuthenticateAsync(
                _emailSettings.SenderEmail,
                _emailSettings.Password);
            await smtpClient.SendAsync(mailMessage);
            await smtpClient.DisconnectAsync(true);
        }
    }
}
