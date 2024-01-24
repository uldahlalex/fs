using System.Threading.RateLimiting;
using Externalities.QueryModels;
using Fleck;

namespace api.Models;

public class WebsocketMetadata
{
    public IWebSocketConnection? Socket { get; set; }
    public bool IsAuthenticated { get; set; }
    public EndUser UserInfo { get; set; } = null!;
    public Dictionary<string, RateLimiter> RateLimitPerEvent { get; set; } = new();
}