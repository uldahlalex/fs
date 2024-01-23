using System.ComponentModel.DataAnnotations;
using System.Threading.RateLimiting;
using api.Abstractions;
using api.StaticHelpers.ExtensionMethods;
using Fleck;

namespace api.Attributes.EventFilters;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class RateLimitAttribute(int eventsPerTimeframe, int secondTimeFrame)
    : BaseEventFilterAttribute
{
    public override Task Handle<T>(IWebSocketConnection socket, T dto)
    {
        var env = Environment.GetEnvironmentVariable("FULLSTACK_SKIP_RATE_LIMITING");
        if (!string.IsNullOrEmpty(env) && env.ToLower().Equals("true"))
            return Task.CompletedTask;

        if (!socket.GetMetadata().RateLimitPerEvent.TryGetValue(dto.eventType, out var rateLimiter))
        {
            socket.GetMetadata().RateLimitPerEvent[dto.eventType] = new FixedWindowRateLimiter(
                new FixedWindowRateLimiterOptions
                {
                    PermitLimit = eventsPerTimeframe,
                    Window = TimeSpan.FromSeconds(secondTimeFrame),
                    AutoReplenishment = true
                });
        }

        var aq = rateLimiter.AcquireAsync().AsTask();
        aq.Wait();
        if (aq.Result.IsAcquired)
            throw new ValidationException("Rate limit exceeded for event " + dto.eventType);
        return Task.CompletedTask;
    }
}