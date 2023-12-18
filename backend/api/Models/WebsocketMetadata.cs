using api.Models.DbModels;
using Fleck;

namespace api.Models;

public class WebsocketMetadata
{
    public IWebSocketConnection? Socket { get; set; }
    public bool IsAuthenticated { get; set; }
    public EndUser UserInfo { get; set; } = null!;
}