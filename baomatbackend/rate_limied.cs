// THANH VIEN 2 - FEATURE 5: ASP.NET CORE RATE LIMITING (LAYER 7 HTTP FLOOD MITIGATION)
// Built-in .NET 8 middleware. Apply endpoint policies and a global IP policy.
// This is an application-layer request limiter, not a substitute for a WAF/CDN/network DDoS service.

using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace BaoMatBackend.RateLimiting;

public sealed class RateLimitOptions
{
    public const string SectionName = "RateLimiting";
    public int GlobalPermitLimit { get; set; } = 120;
    public int GlobalWindowSeconds { get; set; } = 60;
    public int LoginPermitLimit { get; set; } = 5;
    public int LoginWindowSeconds { get; set; } = 20;
    public int OtpWindowMinutes { get; set; } = 5;
    public int OtpPermitLimit { get; set; } = 5;
}

public static class RateLimitingConfiguration
{
    public const string LoginPolicy = "login";
    public const string OtpPolicy = "otp";
    public const string ApiPolicy = "api";

    public static void AddPolicies(IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection(RateLimitOptions.SectionName).Get<RateLimitOptions>()
                       ?? new RateLimitOptions();

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (context, cancellationToken) =>
            {
                await context.HttpContext.Response.WriteAsync(
                    "Too many requests. Please try again later.", cancellationToken);
            };

            // Apply a baseline limit to every routed endpoint, not only login.
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = Math.Max(1, settings.GlobalPermitLimit),
                    Window = TimeSpan.FromSeconds(Math.Max(1, settings.GlobalWindowSeconds)),
                    QueueLimit = 0,
                    AutoReplenishment = true
                });
            });

            options.AddPolicy(LoginPolicy, context =>
            {
                var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = Math.Max(1, settings.LoginPermitLimit),
                    Window = TimeSpan.FromSeconds(Math.Max(1, settings.LoginWindowSeconds)),
                    QueueLimit = 0,
                    AutoReplenishment = true
                });
            });

            options.AddPolicy(OtpPolicy, context =>
            {
                var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = Math.Max(1, settings.OtpPermitLimit),
                    Window = TimeSpan.FromMinutes(Math.Max(1, settings.OtpWindowMinutes)),
                    QueueLimit = 0,
                    AutoReplenishment = true
                });
            });

            options.AddPolicy(ApiPolicy, context =>
            {
                var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return RateLimitPartition.GetSlidingWindowLimiter(ip, _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = 60,
                    Window = TimeSpan.FromMinutes(1),
                    SegmentsPerWindow = 6,
                    QueueLimit = 0,
                    AutoReplenishment = true
                });
            });
        });
    }
}

// Program.cs:
// RateLimitingConfiguration.AddPolicies(builder.Services, builder.Configuration);
// app.UseRouting(); app.UseRateLimiter(); app.UseAuthentication(); app.UseAuthorization();
// Login and OTP endpoints have separate per-IP limits; global baseline covers every routed endpoint.
// IMPORTANT: if behind a trusted reverse proxy, configure ForwardedHeadersOptions with
// explicit KnownProxies/KnownNetworks BEFORE using RemoteIpAddress. Never trust arbitrary X-Forwarded-For.
// Revalidate limits per deployment; combine with Cloudflare/Azure WAF/AWS Shield for volumetric DDoS.
