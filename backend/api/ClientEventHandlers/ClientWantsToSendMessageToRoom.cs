using System.Text.Json;
using api.Abstractions;
using api.Attributes.EventFilters;
using api.Models;
using api.Models.ServerEvents;
using api.State;
using api.StaticHelpers;
using api.StaticHelpers.ExtensionMethods;
using Commons;
using Externalities;
using Externalities._3rdPartyTransferModels;
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
        var skip = Environment.GetEnvironmentVariable("FULLSTACK_SKIP_TOX_FILTER");
        if (skip == "true")
        {
            Log.Information("Toxicity filter is disabled, skipping");
            return;
        }
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
        try
        {
             var analysis =await new AzureCognitiveServices().GetToxicityAnalysis(messageContent);
             var result =analysis.Deserialize<ToxicityResponse>().categoriesAnalysis;
             if (result.Any(x => x.severity > 0.5))
             {
                 var dict = result.ToDictionary(x => x.category, x => x.severity);
                 var response = JsonSerializer.Serialize(dict, new JsonSerializerOptions
                 {
                     WriteIndented = true
                 });
                 Log.Information("Client message was filtered by toxicity filter: " + messageContent +". Analysis result: " + response);

                 throw new ValidationException(response);
             }
             
        } 
        catch (System.ArgumentException e)
        {
            Log.Error(e, "Failed to perform toxicity filtering on message! Sending anyways, you're in dev mode so who cares");
        }

      
    }
}

[RequireAuthentication]
[RateLimit(60, 60)]
public class ClientWantsToSendMessageToRoom(ChatRepository chatRepository)
    : BaseEventHandler<ClientWantsToSendMessageToRoomDto>
{
    public override Task Handle(ClientWantsToSendMessageToRoomDto dto, IWebSocketConnection socket)
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
        var insertedMessage =  chatRepository.InsertMessage(message);
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
        return Task.CompletedTask;
    }
}