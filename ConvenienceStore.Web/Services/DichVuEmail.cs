using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace ConvenienceStore.Web.Services
{
    public class DichVuEmail : IDichVuEmail
    {
        private readonly IConfiguration _configuration;

        public DichVuEmail(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task GuiEmailAsync(string emailNhan, string tieuDe, string noiDungHtml)
        {
            var emailGui = _configuration["EmailSettings:Email"];
            var tenHienThi = _configuration["EmailSettings:DisplayName"];
            var matKhau = _configuration["EmailSettings:Password"];
            var host = _configuration["EmailSettings:Host"];
            var port = int.Parse(_configuration["EmailSettings:Port"]);

            var tinNhan = new MimeMessage();
            tinNhan.From.Add(new MailboxAddress(tenHienThi, emailGui));
            tinNhan.To.Add(MailboxAddress.Parse(emailNhan));
            tinNhan.Subject = tieuDe;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = noiDungHtml
            };

            tinNhan.Body = bodyBuilder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(host, port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(emailGui, matKhau);
            await smtp.SendAsync(tinNhan);
            await smtp.DisconnectAsync(true);
        }
    }
}