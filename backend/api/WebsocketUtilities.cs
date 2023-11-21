using System.ComponentModel.DataAnnotations;
using core;
using Fleck;
using Newtonsoft.Json;
using System.Text.Json;
using Serilog;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace api;

public class WebsocketUtilities(State state)
{
    public void BroadcastMessageToRoom(int roomId, BaseTransferObject transferObject)
    {
        foreach (var socketKeyValuePair in state.AllSockets)
        {
            if (!socketKeyValuePair.Value.GetConnectedRooms().Contains(roomId))
                throw new KeyNotFoundException("User is not present in the room they are trying to send a message to!");
            try
            {
                var exp = state.AllSockets.GetValueOrDefault(socketKeyValuePair.Key) ?? throw new Exception("Could not find socket with GUID "+socketKeyValuePair.Key);
                exp.Send(JsonConvert.SerializeObject(transferObject));
            }
            catch (Exception e)
            {
                Log.Error(e, "WebsocketUtilities");
            }
        }
    }

    public void PurgeClient(IWebSocketConnection socket)
    {
        if (state.AllSockets.ContainsKey(socket.ConnectionInfo.Id))
            state.AllSockets.Remove(socket.ConnectionInfo.Id, out _);
    }

    public void EventNotFound(IWebSocketConnection socket)
    {
        var response = new ServerSendsErrorMessageToClient()
        {
            errorMessage = "Unknown event!"
        };
        socket.Send(JsonConvert.SerializeObject(response));
    }
}

public static class Deserializer<T>
{
    public static T Deserialize(string message)
    {
        return JsonSerializer.Deserialize<T>(message,
                   new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
               ?? throw new DeserializationException($"Failed to deserialize message: {message}");
    }

    public static T DeserializeAndValidate(string message)
    {
        T deserialized = Deserialize(message)!;
        var context = new ValidationContext(deserialized, serviceProvider: null, items: null);
        var validationResults = new List<ValidationResult>();

        bool isValid = Validator.TryValidateObject(deserialized, context, validationResults, true);

        if (!isValid)
        {
            var errors = string.Join(", ", validationResults.Select(rv => rv.ErrorMessage));
            throw new DeserializationException($"Failed to validate message: {message}. Errors: {errors}");
        }

        return deserialized;
    }
}