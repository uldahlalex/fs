// using api.ExtensionMethods;
// using api.Helpers;
// using api.Models;
// using api.Models.ServerEvents;
// using api.State;
// using Infrastructure;
// using Infrastructure.QueryModels;
// using JetBrains.Annotations;
// using MediatR;
//
// namespace api.ClientEventHandlers;
//
// public class ClientWantsToSendMessageToRoom : BaseTransferObject
// {
//     //[ToxicityFilter] 
//     public string? messageContent { get; set; }
//
//     public int roomId { get; set; }
// }
//
// [UsedImplicitly]
// public class ClientWantsToSendMessageToRoomHandler(ChatRepository chatRepository)
//     : IRequestHandler<EventTypeRequest<ClientWantsToSendMessageToRoom>>
// {
//     public Task Handle(EventTypeRequest<ClientWantsToSendMessageToRoom> request, CancellationToken cancellationToken)
//     {
//         SocketUtilities.ExitIfNotAuthenticated(request.Socket, request.MessageObject.eventType);
//         var getValue = WebsocketConnections.TopicSubscriptions.TryGetValue("ChatRooms/" + request.MessageObject.roomId,
//             out var topicSubscriptions);
//         if (!getValue || !topicSubscriptions.Contains(request.Socket.ConnectionInfo.Id))
//             throw new Exception("You are not subscribed to this room");
//
//
//         var insertedMessage =
//             chatRepository.InsertMessage(request.MessageObject.roomId, request.Socket.GetMetadata().UserInfo.id,
//                 request.MessageObject.messageContent!);
//         var messageWithUserInfo = new MessageWithSenderEmail
//         {
//             room = insertedMessage.room,
//             sender = insertedMessage.sender,
//             timestamp = insertedMessage.timestamp,
//             messageContent = insertedMessage.messageContent,
//             id = insertedMessage.id,
//             email = request.Socket.GetMetadata().UserInfo.email
//         };
//         SocketUtilities.BroadcastObjectToTopicListeners(new ServerBroadcastsMessageToClientsInRoom
//         {
//             message = messageWithUserInfo,
//             roomId = request.MessageObject.roomId
//         }, "ChatRooms/" + request.MessageObject.roomId);
//         return Task.CompletedTask;
//     }
// }