using System.Text.Json;
using api.ClientEventHandlers;
using api.Models;
using Websocket.Client;

namespace Tests;

public static class StaticHelpers
{
    public const string Url = "ws://localhost:8181";

    public static ClientWantsToAuthenticateDto AuthEvent = new()
    {
        email = "bla@bla.dk",
        password = "qweqweqwe"
    };

    public static ClientWantsToEnterRoomDto EnterRoomEvent = new()
    {
        roomId = 1
    };

    public static ClientWantsToSendMessageToRoomDto SendMessageEvent = new()
    {
        roomId = 1,
        messageContent = "hey"
    };

    public static string ToJsonString(this object o)
    {
        return JsonSerializer.Serialize(o, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        });
    }

    public static async Task Do<T>(this WebsocketClient ws, T dto, List<(BaseDto dto, string websocketClient)> communication, Func<bool> condition)
        where T : BaseDto
    {
        communication.Add(new(dto, nameof(ws)));
        ws.Send(JsonSerializer.Serialize(dto, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        }));
        WaitForCondition(condition).Wait(); //todo await ot .Wait()
    }
    
    public static async Task WaitForCondition(Func<bool> condition)
    {
        var startTime = DateTime.UtcNow;

        while (!condition())
        {
            var elapsedTime = DateTime.UtcNow - startTime;

            if (elapsedTime > TimeSpan.FromSeconds(10))
            {
                throw new TimeoutException("Condition not met within the specified timeout.");
            }

            Task.Delay(100).Wait();
        }
    }
}
