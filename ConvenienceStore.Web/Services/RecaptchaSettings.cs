namespace ConvenienceStore.Web.Services
{
    public sealed class RecaptchaSettings
    {
        public string SiteKey { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
    }
}
