using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using api.Controllers.Utility;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace api.Controllers;

[ApiController]
[Route("[controller]")]
public class WebSocketController(WebsocketService websocketService) : ControllerBase
{
    [Route("/api/{room}")]
    public async Task Get(string room)
    {
        var context = HttpContext;
        if (context.WebSockets.IsWebSocketRequest)
        {
            WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
            await websocketService.EstablishConnection(webSocket, room);
        }
        else
        {
            context.Response.StatusCode = 400;
        }
    }



}