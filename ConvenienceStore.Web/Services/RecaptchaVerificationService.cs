using System.Text.Json;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace ConvenienceStore.Web.Services
{
    /// <summary>
    /// Xac minh token Google Cloud Fraud Defense reCAPTCHA bang assessments API.
    /// Token chi duoc chap nhan khi Google tra tokenProperties.valid=true va hostname dung.
    /// </summary>
    public sealed class RecaptchaVerificationService : IRecaptchaVerificationService
    {
        private const string VerificationEndpoint = "https://recaptchaenterprise.googleapis.com/v1/projects/{0}/assessments?key={1}";

        private readonly HttpClient _httpClient;
        private readonly RecaptchaSettings _settings;
        private readonly ILogger<RecaptchaVerificationService> _logger;

        public RecaptchaVerificationService(
            HttpClient httpClient,
            IOptions<RecaptchaSettings> settings,
            ILogger<RecaptchaVerificationService> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;
        }

        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(_settings.SiteKey) &&
            !string.IsNullOrWhiteSpace(_settings.ProjectId) &&
            !string.IsNullOrWhiteSpace(_settings.ApiKey);

        public async Task<bool> VerifyAsync(
            string? responseToken,
            string? remoteIp,
            string? userAgent,
            string? expectedHostname,
            CancellationToken cancellationToken = default)
        {
            if (!IsConfigured || string.IsNullOrWhiteSpace(responseToken) ||
                string.IsNullOrWhiteSpace(expectedHostname))
            {
                return false;
            }

            try
            {
                var assessmentRequest = new
                {
                    @event = new
                    {
                        token = responseToken,
                        siteKey = _settings.SiteKey,
                        userIpAddress = remoteIp,
                        userAgent = userAgent ?? string.Empty
                    }
                };

                var endpoint = string.Format(
                    System.Globalization.CultureInfo.InvariantCulture,
                    VerificationEndpoint,
                    Uri.EscapeDataString(_settings.ProjectId),
                    Uri.EscapeDataString(_settings.ApiKey));

                using var response = await _httpClient.PostAsJsonAsync(
                    endpoint,
                    assessmentRequest,
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Google reCAPTCHA verification returned HTTP {StatusCode}.", response.StatusCode);
                    return false;
                }

                await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

                var root = json.RootElement;
                var tokenProperties = root.TryGetProperty("tokenProperties", out var tokenPropertiesElement)
                    ? tokenPropertiesElement
                    : default;

                var success = tokenProperties.ValueKind == JsonValueKind.Object &&
                              tokenProperties.TryGetProperty("valid", out var validElement) &&
                              validElement.ValueKind == JsonValueKind.True;

                var hostname = tokenProperties.ValueKind == JsonValueKind.Object &&
                               tokenProperties.TryGetProperty("hostname", out var hostnameElement)
                    ? hostnameElement.GetString()
                    : null;

                var hostnameMatches = !string.IsNullOrWhiteSpace(hostname) &&
                    string.Equals(hostname, expectedHostname, StringComparison.OrdinalIgnoreCase);

                if (!success || !hostnameMatches)
                {
                    var invalidReason = tokenProperties.ValueKind == JsonValueKind.Object &&
                                        tokenProperties.TryGetProperty("invalidReason", out var invalidReasonElement)
                        ? invalidReasonElement.ToString()
                        : "unknown";

                    _logger.LogInformation(
                        "reCAPTCHA assessment rejected. Valid={Valid}, HostnameMatched={HostnameMatched}, InvalidReason={InvalidReason}.",
                        success,
                        hostnameMatches,
                        invalidReason);
                }

                return success && hostnameMatches;
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Google reCAPTCHA verification timed out.");
                return false;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Could not reach Google reCAPTCHA verification endpoint.");
                return false;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Google reCAPTCHA returned an invalid JSON response.");
                return false;
            }
        }
    }
}
