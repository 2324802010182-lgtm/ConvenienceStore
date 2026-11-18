namespace ConvenienceStore.Web.ViewModels
{
    public class EmailSettings
    {
        // Dia chi hop thu dung de gui email thong bao (sender), khong phai nguoi nhan.
        public string SenderEmail { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
    }
}
