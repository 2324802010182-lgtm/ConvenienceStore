// THANH VIEN 2 - FEATURE 2: EMAIL OTP / TWO-FACTOR AUTHENTICATION
// This module implements a separate second factor after password verification.
// It uses cryptographically secure 6-digit codes, hashed OTP storage, expiry,
// one-time consumption, attempt limits, and ASP.NET Core Identity temporary 2FA sign-in.
// Requires Identity AddDefaultTokenProviders() and an email sender implementation.

using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using ConvenienceStore.Models.Entities;

namespace BaoMatBackend.TwoFactor;

public sealed class EmailOtpOptions
{
    public const string SectionName = "EmailOtp";
    public int CodeLifetimeMinutes { get; set; } = 5;
    public int MaxVerificationAttempts { get; set; } = 5;
    public int ResendCooldownSeconds { get; set; } = 60;
}

public interface IOtpEmailSender
{
    Task SendAsync(string email, string code, CancellationToken cancellationToken = default);
}

// Adapter to the application's existing MailKit email service.
public sealed class MailKitOtpEmailSender(ConvenienceStore.Web.Services.IDichVuEmail emailService) : IOtpEmailSender
{
    public Task SendAsync(string email, string code, CancellationToken cancellationToken = default) =>
        emailService.GuiEmailAsync(email, "Mã OTP xác thực đăng nhập - Convenience Store",
            $"<h3>Xác thực đăng nhập</h3><p>Mã OTP của bạn: <strong>{System.Net.WebUtility.HtmlEncode(code)}</strong></p><p>Mã có hiệu lực trong 5 phút. Không chia sẻ mã này với bất kỳ ai.</p>");
}

public interface IEmailOtpService
{
    Task SendAsync(string userId, string email, CancellationToken cancellationToken = default);
    Task<bool> VerifyAsync(string userId, string code, CancellationToken cancellationToken = default);
}

public sealed class EmailOtpService(
    UserManager<NguoiDung> userManager,
    IOtpEmailSender emailSender,
    IOptions<EmailOtpOptions> options,
    ILogger<EmailOtpService> logger) : IEmailOtpService
{
    private const string Provider = "BaoMatBackend.EmailOtp";
    private const string HashName = "OtpHash";
    private const string ExpiryName = "OtpExpiry";
    private const string AttemptsName = "OtpAttempts";
    private const string LastSentName = "OtpLastSent";
    private const string PendingName = "OtpPending";
    private const int Pbkdf2Iterations = 150_000;
    private readonly EmailOtpOptions _options = options.Value;

    public async Task SendAsync(string userId, string email, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId) ?? throw new InvalidOperationException("User not found.");
        var previous = await userManager.GetAuthenticationTokenAsync(user, Provider, LastSentName);
        if (long.TryParse(previous, out var ticks) &&
            DateTimeOffset.UtcNow - new DateTimeOffset(ticks, TimeSpan.Zero) < TimeSpan.FromSeconds(_options.ResendCooldownSeconds))
            throw new InvalidOperationException("Please wait before requesting another OTP.");

        var code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
        var salt = RandomNumberGenerator.GetBytes(16);
        // OTPs have only one million possible values. PBKDF2 slows offline guessing if the
        // user-token table is exposed; a plain SHA-256 hash would be too cheap to brute-force.
        var derived = Rfc2898DeriveBytes.Pbkdf2(
            code, salt, Pbkdf2Iterations, HashAlgorithmName.SHA256, outputLength: 32);
        var hash = Convert.ToBase64String(salt) + "." + Convert.ToBase64String(derived);
        var expiry = DateTimeOffset.UtcNow.AddMinutes(_options.CodeLifetimeMinutes);

        await userManager.SetAuthenticationTokenAsync(user, Provider, HashName, hash);
        await userManager.SetAuthenticationTokenAsync(user, Provider, ExpiryName, expiry.UtcTicks.ToString());
        await userManager.SetAuthenticationTokenAsync(user, Provider, AttemptsName, "0");
        await userManager.SetAuthenticationTokenAsync(user, Provider, LastSentName, DateTimeOffset.UtcNow.UtcTicks.ToString());
        await userManager.SetAuthenticationTokenAsync(user, Provider, PendingName, "1");

        try { await emailSender.SendAsync(email, code, cancellationToken); }
        catch
        {
            await ClearAsync(user);
            await userManager.RemoveAuthenticationTokenAsync(user, Provider, LastSentName);
            throw;
        }
    }

    public async Task<bool> VerifyAsync(string userId, string code, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null || string.IsNullOrWhiteSpace(code) || code.Length != 6 || !code.All(char.IsDigit)) return false;

        var stored = await userManager.GetAuthenticationTokenAsync(user, Provider, HashName);
        var expiryText = await userManager.GetAuthenticationTokenAsync(user, Provider, ExpiryName);
        var attemptsText = await userManager.GetAuthenticationTokenAsync(user, Provider, AttemptsName);
        var pending = await userManager.GetAuthenticationTokenAsync(user, Provider, PendingName);
        if (pending != "1" || stored is null || !long.TryParse(expiryText, out var expiryTicks)) return false;
        if (DateTimeOffset.UtcNow.UtcTicks > expiryTicks) { await ClearAsync(user); return false; }

        var attempts = int.TryParse(attemptsText, out var count) ? count : 0;
        if (attempts >= _options.MaxVerificationAttempts) { await ClearAsync(user); return false; }

        var parts = stored.Split('.', 2);
        if (parts.Length != 2) { await ClearAsync(user); return false; }
        byte[] salt, expected;
        try { salt = Convert.FromBase64String(parts[0]); expected = Convert.FromBase64String(parts[1]); }
        catch (FormatException) { await ClearAsync(user); return false; }
        var actual = Rfc2898DeriveBytes.Pbkdf2(
            code, salt, Pbkdf2Iterations, HashAlgorithmName.SHA256, outputLength: 32);
        if (!CryptographicOperations.FixedTimeEquals(expected, actual))
        {
            attempts++;
            logger.LogWarning("Rejected an invalid email OTP for user {UserId}; attempt {Attempt}.", userId, attempts);
            await userManager.SetAuthenticationTokenAsync(user, Provider, AttemptsName, attempts.ToString());
            if (attempts >= _options.MaxVerificationAttempts) await ClearAsync(user);
            return false;
        }

        await ClearAsync(user); // OTP cannot be reused.
        return true;
    }

    private async Task ClearAsync(NguoiDung user)
    {
        await userManager.RemoveAuthenticationTokenAsync(user, Provider, HashName);
        await userManager.RemoveAuthenticationTokenAsync(user, Provider, ExpiryName);
        await userManager.RemoveAuthenticationTokenAsync(user, Provider, AttemptsName);
        await userManager.RemoveAuthenticationTokenAsync(user, Provider, PendingName);
    }
}

// REGISTER with your project's concrete user type (NguoiDung):
// Configure<EmailOtpOptions>(Configuration.GetSection("EmailOtp"));
// AddScoped<IEmailOtpService, EmailOtpService>();
// AddScoped<IOtpEmailSender, YourMailKitOtpEmailSender>();
// This project uses NguoiDung, so this service is already typed to the correct user class.
// LOGIN FLOW:
// 1) PasswordSignInAsync(..., lockoutOnFailure:true). If RequiresTwoFactor, issue OTP to verified email.
// 2) Keep user id in a short-lived protected flow (Identity 2FA cookie or server session).
// 3) On OTP POST call VerifyAsync(user.Id, postedCode); only then call SignInAsync(user,...).
// Never sign in the user between password success and OTP verification.
// Account enrollment/enablement must require a verified email and explicit user consent.
