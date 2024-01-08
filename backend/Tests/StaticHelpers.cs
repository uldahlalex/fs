

using System.Text.Json;
using api.Models;
using Websocket.Client;

namespace Tests;

public static class StaticHelpers
{
    public const string Url = "ws://localhost:8181";
    public static string ToJsonString(this object o)
    {
        return JsonSerializer.Serialize(o, new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        });
    }

    public static async Task Do<T>(this WebsocketClient ws, T dto, List<Tuple<BaseDto, string>> communication) where T : BaseDto
    {
        communication.Add(new Tuple<BaseDto, string>(dto, nameof(ws)));
        ws.Send(JsonSerializer.Serialize(dto, new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
        }));
        await Task.Delay(100);
    }
  
}