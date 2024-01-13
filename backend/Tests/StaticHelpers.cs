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

    public static async Task Do<T>(this WebsocketClient ws, T dto, List<Tuple<BaseDto, string>> communication)
        where T : BaseDto
    {
        communication.Add(new Tuple<BaseDto, string>(dto, nameof(ws)));
        ws.Send(JsonSerializer.Serialize(dto, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        }));
        Task.Delay(1000).Wait();
    }
}