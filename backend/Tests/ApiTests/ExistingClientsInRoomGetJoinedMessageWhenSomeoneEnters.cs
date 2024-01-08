using System.Collections.ObjectModel;
using api.ClientEventHandlers;
using api.Extensions;
using api.Models;
using api.Models.ServerEvents;
using FluentAssertions;
using NUnit.Framework;
using Serilog;
using WebSocketSharp;

namespace Tests.ApiTests;

public class ExistingClientsInRoomGetJoinedMessageWhenSomeoneEnters
{
    

    //todo refactor to websocket.client
    [Test]
    public async Task Existing_Clients_In_Room_Get_Joined_Message_When_Someone_Enters()
    {
        using (var ws = new WebSocket(StaticHelpers.Url))
        using (var ws2 = new WebSocket(StaticHelpers.Url))
        {
            var messagesReceivedFromServer = new List<BaseDto>();
            ws.Connect();
            ws2.Connect();
            ws.OnMessage += (sender, e) =>
                messagesReceivedFromServer.Add(e.Data.DeserializeAndValidate<BaseDto>());
            ws.OnError += (sender, e) => { Assert.Fail(); };

            ws2.OnMessage += (sender, e) =>
                messagesReceivedFromServer.Add(e.Data.DeserializeAndValidate<BaseDto>());
            ws2.OnError += (sender, e) => { Assert.Fail(); };
            var auth = new ClientWantsToAuthenticateDto
            {
                email = "bla@bla.dk",
                password = "qweqweqwe"
            }.ToJsonString();
            ws.Send(auth);
            ws2.Send(auth);
            var enterRoom = new ClientWantsToEnterRoomDto
            {
                roomId = 1
            }.ToJsonString();
            ws.Send(enterRoom);
            await Task.Delay(2000);
            ws2.Send(enterRoom);


            await Task.Delay(2000);
            Log.Information("Messages: " + messagesReceivedFromServer.ToJsonString());
            messagesReceivedFromServer.Should()
                .Contain(x => x.eventType == nameof(ServerNotifiesClientsInRoomSomeoneHasJoinedRoom));
            messagesReceivedFromServer.Should().NotContain(x => x.eventType == nameof(ServerSendsErrorMessageToClient));
        }
    }

    
    
}