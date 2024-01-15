using api.Extensions;
using api.Models;
using api.Models.ServerEvents;
using FluentAssertions;
using NUnit.Framework;
using Websocket.Client;

namespace Tests.ApiTests;

[TestFixture]
public class ExistingRoomGetsMessageWhenUserJoins
{
    [Test]
    public async Task Message_Broadcasting_In_Room_works()
    {
        using (var ws = new WebsocketClient(new Uri(StaticHelpers.Url)))
        using (var ws2 = new WebsocketClient(new Uri(StaticHelpers.Url)))
        {
            var wsAndHistory = await ws.SetupWsClient();
            var ws2AndHistory = await ws2.SetupWsClient();

            await wsAndHistory.DoAndWaitUntil(StaticHelpers.AuthEvent,  wsAndHistory.communication.AreTheseDtosPresent([
                (typeof(ServerAuthenticatesUser), 1)
            ]));
            await ws2AndHistory.DoAndWaitUntil(StaticHelpers.AuthEvent,  wsAndHistory.communication.AreTheseDtosPresent([
                (typeof(ServerAuthenticatesUser), 1)
            ]));
            

            await wsAndHistory.DoAndWaitUntil(StaticHelpers.EnterRoomEvent,  wsAndHistory.communication.AreTheseDtosPresent([
                (typeof(ServerAddsClientToRoom), 1),
                (typeof(ServerNotifiesClientsInRoomSomeoneHasJoinedRoom), 1),
                (typeof(ServerSendsErrorMessageToClient), 0)

            ]));
            await ws2AndHistory.DoAndWaitUntil(StaticHelpers.EnterRoomEvent,  wsAndHistory.communication.AreTheseDtosPresent([
                (typeof(ServerAddsClientToRoom), 2),
                (typeof(ServerNotifiesClientsInRoomSomeoneHasJoinedRoom), 3),
                (typeof(ServerSendsErrorMessageToClient), 0)

            ]));
            
            await wsAndHistory.DoAndWaitUntil(StaticHelpers.SendMessageEvent,  wsAndHistory.communication.AreTheseDtosPresent([
                (typeof(ServerBroadcastsMessageToClientsInRoom), 2),
            ]));         
            await ws2AndHistory.DoAndWaitUntil(StaticHelpers.SendMessageEvent,  wsAndHistory.communication.AreTheseDtosPresent([
                (typeof(ServerBroadcastsMessageToClientsInRoom), 4),
            ]));

     
        }
    }
}