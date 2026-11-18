// THANH VIEN 2 - FEATURE 3: LOGIN LOCKOUT / PROGRESSIVE LOCKOUT
// ASP.NET Core Identity built-in lockout plus explicit safe configuration.
// Integrate inside AddIdentity<NguoiDung, IdentityRole>(options => { ... }).

using Microsoft.AspNetCore.Identity;

namespace BaoMatBackend.Lockout;

public static class LockoutConfiguration
{
    public static void Configure(IdentityOptions options)
    {
        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 3;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromSeconds(20);
    }

    // Invoke for existing accounts on registration/migration where necessary:
    // await userManager.SetLockoutEnabledAsync(user, true);
    // Sign-in must use PasswordSignInAsync(userName, password, isPersistent, lockoutOnFailure: true).
}

public sealed class ProgressiveLockoutOptions
{
    public int InitialSeconds { get; set; } = 20;
    public int MaximumSeconds { get; set; } = 20;
}

public static class ProgressiveLockout
{
    // Calculate a bounded exponential lockout duration after Identity reports IsLockedOut.
    // Demo configuration: every lockout lasts 20 seconds.
    public static TimeSpan GetDuration(int stage, ProgressiveLockoutOptions? options = null)
    {
        options ??= new ProgressiveLockoutOptions();
        if (options.InitialSeconds < 1 || options.MaximumSeconds < options.InitialSeconds)
            throw new ArgumentOutOfRangeException(nameof(options));
        var seconds = (double)options.InitialSeconds * Math.Pow(2, Math.Clamp(stage, 0, 30));
        return TimeSpan.FromSeconds(Math.Min(seconds, options.MaximumSeconds));
    }
}

// Controller pattern:
// var result = await signInManager.PasswordSignInAsync(user.UserName!, password,
//     isPersistent: rememberMe, lockoutOnFailure: true);
// if (result.IsLockedOut) { /* show generic locked message; log user id and event */ }
// Do not reveal whether an email exists. Apply an independent IP-based rate limit as well.
// Security note: avoid repeatedly resetting failed-count/lockout timestamps from client input.
