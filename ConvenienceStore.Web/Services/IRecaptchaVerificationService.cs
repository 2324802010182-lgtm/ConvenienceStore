namespace ConvenienceStore.Web.Services
{
    public interface IRecaptchaVerificationService
    {
        bool IsConfigured { get; }

        Task<bool> VerifyAsync(
            string? responseToken,
            string? remoteIp,
            string? userAgent,
            string? expectedHostname,
            CancellationToken cancellationToken = default);
    }
}
