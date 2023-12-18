// using System.ComponentModel.DataAnnotations;
// using api.ExtensionMethods;
// using api.Helpers;
// using api.Models;
// using api.Models.ServerEvents;
// using api.State;
// using Infrastructure;
// using JetBrains.Annotations;
// using MediatR;
//
// namespace api.ClientEventHandlers;
//
// public class ClientWantsToEnterRoom : BaseTransferObject
// {
//     [Required] [Range(1, int.MaxValue)] public int roomId { get; set; }
// }
//
// [UsedImplicitly]
// public class ClientWantsToEnterRoomHandler(ChatRepository chatRepository)
//     : IRequestHandler<EventTypeRequest<ClientWantsToEnterRoom>>
// {
//     public Task Handle(EventTypeRequest<ClientWantsToEnterRoom> request, CancellationToken cancellationToken)
//     {
//         SocketUtilities.ExitIfNotAuthenticated(request.Socket, request.MessageObject.eventType);
//         SocketUtilities.BroadcastObjectToTopicListeners(new ServerNotifiesClientsInRoomSomeoneHasJoinedRoom
//         {
//             message = "Client joined the room!",
//             user = request.Socket.GetMetadata().UserInfo,
//             roomId = request.MessageObject.roomId
//         }, "ChatRooms/" + request.MessageObject.roomId);
//         request.Socket.SubscribeToTopic("ChatRooms/" + request.MessageObject.roomId);
//         request.Socket.SendDto(new ServerAddsClientToRoom
//         {
//             messages = chatRepository.GetPastMessages(request.MessageObject.roomId),
//             liveConnections =
//                 WebsocketConnections.TopicSubscriptions["ChatRooms/" + request.MessageObject.roomId]
//                     .Count, //request.Socket.CountUsersInRoom(request.MessageObject.roomId.ToString()),
//             roomId = request.MessageObject.roomId
//         });
//         return Task.CompletedTask;
//     }
// }