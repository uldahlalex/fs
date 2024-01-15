using api.Extensions;
using api.Models;
using api.Models.ServerEvents;
using FluentAssertions;
using NUnit.Framework;
using Websocket.Client;

namespace Tests.ApiTests;

[TestFixture]
public class SignInEnterRoomSendMessage
{
    
    [Test]
    public async Task Signin_EnterRoom_SendMessage()
    {
        using (var ws = new WebsocketClient(new Uri(StaticHelpers.Url)))
        {
            var wsAndHistory = await ws.SetupWsClient();
            

            await wsAndHistory.DoAndWaitUntil(StaticHelpers.AuthEvent,  wsAndHistory.communication.AreTheseDtosPresent([
                    (typeof(ServerAuthenticatesUser), 1)
                ]));

            await wsAndHistory.DoAndWaitUntil(StaticHelpers.EnterRoomEvent,  wsAndHistory.communication.AreTheseDtosPresent([
            (typeof(ServerAddsClientToRoom), 1),
            (typeof(ServerNotifiesClientsInRoomSomeoneHasJoinedRoom), 1),
            (typeof(ServerSendsErrorMessageToClient), 0)

            ]));

            await wsAndHistory.DoAndWaitUntil(StaticHelpers.SendMessageEvent,
                wsAndHistory.communication.AreTheseDtosPresent([
                    (typeof(ServerBroadcastsMessageToClientsInRoom), 1),
                    (typeof(ServerSendsErrorMessageToClient), 0)
                ]));

        }
    }
}