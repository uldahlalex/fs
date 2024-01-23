using System.ComponentModel.DataAnnotations;
using System.Threading.RateLimiting;
using api.Abstractions;
using api.StaticHelpers.ExtensionMethods;
using Fleck;

namespace api.Attributes.EventFilters;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class RateLimitAttribute(int eventsPerTimeframe, int secondTimeFrame) : BaseEventFilterAttribute
{
    public override async Task Handle<T>(IWebSocketConnection socket, T dto)
    {
        var env = Environment.GetEnvironmentVariable("FULLSTACK_SKIP_RATE_RATE_LIMITING");
        if (!string.IsNullOrEmpty(env) && env.ToLower().Equals("true"))
            return;

        var metadata = socket.GetMetadata();
        if (!metadata.RateLimitPerEvent.TryGetValue(dto.eventType, out var rateLimiter))
        {
            rateLimiter = new FixedWindowRateLimiter(new FixedWindowRateLimiterOptions
            {
                PermitLimit = eventsPerTimeframe,
                Window = TimeSpan.FromSeconds(secondTimeFrame),
                AutoReplenishment = true
            });
            metadata.RateLimitPerEvent[dto.eventType] = rateLimiter;
        }

        var lease = await rateLimiter.AcquireAsync(1);
        if (!lease.IsAcquired)
        {
            throw new ValidationException("Rate limit exceeded for event " + dto.eventType);
        }
    }
}