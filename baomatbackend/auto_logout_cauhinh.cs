// THANH VIEN 2 - FEATURE 4: AUTO LOGOUT AFTER INACTIVITY
// Authentication cookie expiration is the authoritative login lifetime. Session timeout
// is separate and does NOT automatically sign the user out of ASP.NET Core Identity.

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace BaoMatBackend.AutoLogout;

public sealed class AutoLogoutOptions
{
    public const string SectionName = "AutoLogout";
    public int IdleSeconds { get; set; } = 20;
    public bool SlidingExpiration { get; set; } = true;
}

public static class AutoLogoutConfiguration
{
    // Call after AddIdentity(...) in Program.cs:
    // builder.Services.Configure<SecurityStampValidatorOptions>(o =>
    //     o.ValidationInterval = TimeSpan.FromMinutes(2));
    // builder.Services.ConfigureApplicationCookie(options => Configure(options, 20));
    public static void Configure(CookieAuthenticationOptions options, int idleSeconds, bool slidingExpiration = true, bool requireHttps = true)
    {
        if (idleSeconds < 1) throw new ArgumentOutOfRangeException(nameof(idleSeconds));
        options.ExpireTimeSpan = TimeSpan.FromSeconds(idleSeconds);
        options.SlidingExpiration = slidingExpiration;
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        // The local launch profile uses plain HTTP by default; keep the cookie usable there.
        // Production deployments must run behind HTTPS and always mark the cookie Secure.
        options.Cookie.SecurePolicy = requireHttps ? CookieSecurePolicy.Always : CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Events.OnRedirectToLogin = context =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            }
            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };
    }
}

// If the requirement means a strict idle cutoff, not a renewable absolute cookie lifetime,
// use server-side last-activity tracking and validate it on every authenticated request.
// Middleware below provides that behavior using distributed cache; register IDistributedCache
// (Redis for multi-instance deployment; memory cache is development-only).
public sealed class IdleLogoutMiddleware
{
    private const string LastActivityKey = "BaoMatBackend.LastActivityUtc";
    private readonly RequestDelegate _next;
    private readonly ILogger<IdleLogoutMiddleware> _logger;
    private readonly TimeSpan _idleLimit;

    public IdleLogoutMiddleware(RequestDelegate next, ILogger<IdleLogoutMiddleware> logger, IOptions<AutoLogoutOptions> options)
    {
        _next = next;
        _logger = logger;
        _idleLimit = TimeSpan.FromSeconds(Math.Max(1, options.Value.IdleSeconds));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var cache = context.RequestServices.GetRequiredService<Microsoft.Extensions.Caching.Distributed.IDistributedCache>();
            var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                // Include the ASP.NET session id so activity on another device/browser
                // cannot keep this browser session alive.
                var sessionId = context.Session.Id;
                var key = $"{LastActivityKey}:{userId}:{sessionId}";
                var lastText = await cache.GetStringAsync(key, context.RequestAborted);
                if (DateTimeOffset.TryParse(lastText, out var last) && DateTimeOffset.UtcNow - last > _idleLimit)
                {
                    await context.SignOutAsync(IdentityConstants.ApplicationScheme);
                    // Remove the old activity marker immediately so a fresh login in
                    // the same browser session is not rejected by stale timeout state.
                    await cache.RemoveAsync(key, context.RequestAborted);
                    _logger.LogInformation("User {UserId} signed out due to inactivity.", userId);
                    if (context.Request.Path.StartsWithSegments("/api"))
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return;
                    }
                    context.Response.Redirect("/TaiKhoan/DangNhap?reason=timeout");
                    return;
                }
                await cache.SetStringAsync(key, DateTimeOffset.UtcNow.ToString("O"),
                    new Microsoft.Extensions.Caching.Distributed.DistributedCacheEntryOptions
                    // Keep the activity marker long enough to compare on the next request.
                    { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24) }, context.RequestAborted);
            }
        }
        await _next(context);
    }
}

// Pipeline: app.UseAuthentication(); app.UseMiddleware<IdleLogoutMiddleware>(); app.UseAuthorization();
// Ensure the middleware runs AFTER UseAuthentication and BEFORE UseAuthorization.
