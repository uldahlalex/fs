using System.Threading.RateLimiting;
using api.Models.DbModels;
using Fleck;

namespace api.Models;

public class WebsocketMetadata
{
    public IWebSocketConnection? Socket { get; set; }
    public bool IsAuthenticated { get; set; }
    public EndUser UserInfo { get; set; } = null!;
    public RateLimiter RateLimiter { get; set; } //todo rate limiter is for all events and for each connection; can be revised
}