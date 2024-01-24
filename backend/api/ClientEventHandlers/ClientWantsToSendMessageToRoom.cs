using System.Text.Json;
using api.Abstractions;
using api.Attributes.EventFilters;
using api.Models;
using api.Models.ServerEvents;
using api.State;
using api.StaticHelpers;
using api.StaticHelpers.ExtensionMethods;
using Externalities;
using Externalities.QueryModels;
using Fleck;
using Serilog;
using ValidationException = System.ComponentModel.DataAnnotations.ValidationException;

namespace api.ClientEventHandlers;

public class ClientWantsToSendMessageToRoomDto : BaseDto
{
    public string? messageContent { get; set; }

    public int roomId { get; set; }

    public override async Task ValidateAsync()
    {
        var azKey = Environment.GetEnvironmentVariable("FULLSTACK_AZURE_COGNITIVE_SERVICES");
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if (env != null && env.ToLower().Equals("development") && string.IsNullOrEmpty(azKey))
        {
            Log.Information("Skipping toxicity filter in development mode when no API key is provided");
            return;
        }

        if (Environment.GetEnvironmentVariable("FULLSTACK_SKIP_TOXICITY_FILTER")?.ToLower().Equals("true") ?? false)
        {
            Log.Information("Skipping toxicity filter in development mode when no API key is provided");
            return;
        }

        // Use await instead of Wait() and Result
        var result = await new AzureCognitiveServices().IsToxic(messageContent);

        Log.Information(JsonSerializer.Serialize(result, new JsonSerializerOptions
        {
            WriteIndented = true
        }));

        if (result.Any(x => x.severity > 0.5))
        {
            var dict = result.ToDictionary(x => x.category, x => x.severity);
            var response = JsonSerializer.Serialize(dict, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            throw new ValidationException(response);
        }
    }
}

[RequireAuthentication]
[RateLimit(5, 60)]
public class ClientWantsToSendMessageToRoom(ChatRepository chatRepository)
    : BaseEventHandler<ClientWantsToSendMessageToRoomDto>
{
    public override async Task Handle(ClientWantsToSendMessageToRoomDto dto, IWebSocketConnection socket)
    {
        var topic = dto.roomId.ParseTopicFromRoomId();
        var getValue = WebsocketConnections.TopicSubscriptions.TryGetValue(topic,
            out var topicSubscriptions);
        if (!getValue || !topicSubscriptions!.Contains(socket.ConnectionInfo.Id))
            throw new Exception("You are not subscribed to this room");
        var message = new Message
        {
            timestamp = DateTimeOffset.UtcNow,
            room = dto.roomId,
            sender = socket.GetMetadata().UserInfo.id,
            messageContent = dto.messageContent!
        };
        var insertedMessage = await chatRepository.InsertMessage(message);
        var messageWithUserInfo = new MessageWithSenderEmail //todo this is kinda ugly ngl
        {
            room = insertedMessage.room,
            sender = insertedMessage.sender,
            timestamp = insertedMessage.timestamp,
            messageContent = insertedMessage.messageContent,
            id = insertedMessage.id,
            email = socket.GetMetadata().UserInfo.email
        };
        StaticWebSocketHelpers.BroadcastObjectToTopicListeners(new ServerBroadcastsMessageToClientsInRoom
        {
            message = messageWithUserInfo,
            roomId = dto.roomId
        }, topic);
    }
}