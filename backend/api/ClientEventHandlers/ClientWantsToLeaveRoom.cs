// using api.ExtensionMethods;
// using api.Helpers;
// using api.Models;
// using api.Models.ServerEvents;
// using JetBrains.Annotations;
// using MediatR;
//
// namespace api.ClientEventHandlers;
//
// public class ClientWantsToLeaveRoom : BaseTransferObject
// {
//     public int roomId { get; set; }
// }
//
// [UsedImplicitly]
// public class ClientWantsToLeaveRoomHandler : IRequestHandler<EventTypeRequest<ClientWantsToLeaveRoom>>
// {
//     public Task Handle(EventTypeRequest<ClientWantsToLeaveRoom> request, CancellationToken cancellationToken)
//     {
//         request.Socket.UnsubscribeFromTopic(request.MessageObject.roomId.ToString());
//         SocketUtilities.BroadcastObjectToTopicListeners(new ServerNotifiesClientsInRoomSomeoneHasLeftRoom
//                 { user = request.Socket.GetMetadata().UserInfo },
//             request.MessageObject.roomId.ToString());
//         return Task.CompletedTask;
//     }
// }