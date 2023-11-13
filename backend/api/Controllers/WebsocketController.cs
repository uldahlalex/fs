using System.Net.WebSockets;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[ApiController]
[Route("[controller]")]
public class WebSocketController() : ControllerBase
{
    [Route("/api/{room}")]
    public async Task Get(string room)
    {
    
    }
}