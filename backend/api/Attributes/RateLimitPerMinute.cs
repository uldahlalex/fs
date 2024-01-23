using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;

namespace api.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class RateLimitAttribute : Attribute
{
    public int EventsPerTimeframe { get; }
    public TimeSpan Timeframe { get; }

    public RateLimitAttribute(int eventsPerTimeframe, int timeframeInSeconds)
    {
        EventsPerTimeframe = eventsPerTimeframe;
        Timeframe = TimeSpan.FromSeconds(timeframeInSeconds);
    }
}