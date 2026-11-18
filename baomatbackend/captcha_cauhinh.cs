// THANH VIEN 2 - FEATURE 1: GOOGLE reCAPTCHA v2 CHECKBOX
// Does not require a Google Cloud Enterprise project. Register a v2 Checkbox site key
// and secret key at https://www.google.com/recaptcha/admin/create.
// Store the secret key in User Secrets / environment variables; never commit it.

using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace BaoMatBackend.Captcha;

public sealed class RecaptchaOptions
{
    public const string SectionName = "Recaptcha";
    public string SiteKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
}

public sealed record RecaptchaResult(bool Success, string Reason)
{
    public static RecaptchaResult Reject(string reason) => new(false, reason);
}

public interface IRecaptchaV2Verifier
{
    bool IsConfigured { get; }
    Task<RecaptchaResult> VerifyAsync(string? responseToken, string? remoteIp, string? expectedHostname,
        CancellationToken cancellationToken = default);
}

internal sealed class RecaptchaV2SiteVerifyResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("hostname")]
    public string? Hostname { get; set; }

    [JsonPropertyName("challenge_ts")]
    public DateTimeOffset? ChallengeTimestamp { get; set; }

    [JsonPropertyName("error-codes")]
    public string[] ErrorCodes { get; set; } = [];
}

public sealed class RecaptchaV2Verifier(
    HttpClient http,
    IOptions<RecaptchaOptions> options,
    ILogger<RecaptchaV2Verifier> logger) : IRecaptchaV2Verifier
{
    private const string VerifyEndpoint = "https://www.google.com/recaptcha/api/siteverify";
    private readonly RecaptchaOptions _options = options.Value;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(_options.SiteKey) &&
        !string.IsNullOrWhiteSpace(_options.SecretKey);

    public async Task<RecaptchaResult> VerifyAsync(
        string? responseToken,
        string? remoteIp,
        string? expectedHostname,
        CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
            return RecaptchaResult.Reject("reCAPTCHA v2 chưa được cấu hình Site Key/Secret Key.");

        if (string.IsNullOrWhiteSpace(responseToken))
            return RecaptchaResult.Reject("Bạn chưa xác nhận reCAPTCHA.");
        if (string.IsNullOrWhiteSpace(expectedHostname))
            return RecaptchaResult.Reject("Không xác định được hostname để xác minh reCAPTCHA.");

        try
        {
            var formValues = new Dictionary<string, string>
            {
                ["secret"] = _options.SecretKey,
                ["response"] = responseToken
            };
            if (!string.IsNullOrWhiteSpace(remoteIp))
                formValues["remoteip"] = remoteIp;

            using var content = new FormUrlEncodedContent(formValues);

            using var response = await http.PostAsync(VerifyEndpoint, content, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Google reCAPTCHA v2 siteverify returned HTTP {StatusCode}.", response.StatusCode);
                return RecaptchaResult.Reject("Không thể kết nối dịch vụ xác minh reCAPTCHA.");
            }

            var result = await response.Content.ReadFromJsonAsync<RecaptchaV2SiteVerifyResponse>(
                cancellationToken: cancellationToken);

            if (result is null || !result.Success)
            {
                logger.LogInformation("reCAPTCHA v2 rejected. ErrorCodes={ErrorCodes}",
                    result?.ErrorCodes is { Length: > 0 } errors ? string.Join(",", errors) : "none");
                return RecaptchaResult.Reject("Xác minh reCAPTCHA không thành công hoặc token đã hết hạn.");
            }

            if (!string.Equals(result.Hostname, expectedHostname, StringComparison.OrdinalIgnoreCase))
            {
                logger.LogWarning("reCAPTCHA v2 hostname mismatch. Expected={ExpectedHost}, Returned={ReturnedHost}",
                    expectedHostname, result.Hostname);
                return RecaptchaResult.Reject("Hostname của reCAPTCHA không khớp với website.");
            }

            return new RecaptchaResult(true, "Verified");
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("Google reCAPTCHA v2 verification timed out.");
            return RecaptchaResult.Reject("Dịch vụ reCAPTCHA phản hồi quá thời gian.");
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Could not reach Google reCAPTCHA v2 siteverify.");
            return RecaptchaResult.Reject("Không thể kết nối dịch vụ reCAPTCHA.");
        }
        catch (System.Text.Json.JsonException ex)
        {
            logger.LogError(ex, "Google reCAPTCHA v2 returned an invalid response.");
            return RecaptchaResult.Reject("Phản hồi xác minh reCAPTCHA không hợp lệ.");
        }
    }
}
