using System.Text.Json;
using Fleck;

namespace api;

public static class Deserializer<T>
{
    public static T Deserialize(string message, IWebSocketConnection socket)
    {
        return JsonSerializer.Deserialize<T>(message,
                   new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
               ?? throw new DeserializationException($"Failed to deserialize message: {message}");
    }
}