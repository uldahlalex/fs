using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using api.Models.Enums;
using api.StaticHelpers.ExtensionMethods;
using Fleck;

namespace api.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class RateLimitAttribute : Attribute
{
    public static async Task ValidateRateLimit(IWebSocketConnection socket)
    {
        var env = Environment.GetEnvironmentVariable("FULLSTACK_SKIP_RATE_LIMITING");
        if(!string.IsNullOrEmpty(env) && env.ToLower().Equals("true")) 
            return;
        var rateLimiter = socket.GetMetadata().RateLimiter;
        var lease = await rateLimiter.AcquireAsync();
        if (!lease.IsAcquired)
            throw new ValidationException("Rate limit exceeded");
    }
}