using api.Extensions;
using api.Models;
using api.Models.ServerEvents;
using FluentAssertions;
using NUnit.Framework;
using Websocket.Client;

namespace Tests.ApiTests;

[TestFixture]
public class ExistingClientsInRoomGetJoinedMessageWhenSomeoneEnters
{
    [Test]
    public async Task Existing_Clients_In_Room_Get_Joined_Message_When_Someone_Enters()
    {
        using (var ws = new WebsocketClient(new Uri(StaticHelpers.Url)))
        using (var ws2 = new WebsocketClient(new Uri(StaticHelpers.Url)))
        {
            var communication = new List<Tuple<BaseDto, string>>();

            ws.MessageReceived.Subscribe(msg =>
            {
                communication.Add(
                    new Tuple<BaseDto, string>(msg.Text.DeserializeAndValidate<BaseDto>(), nameof(ws)));
            });

            await ws.Start();
            await ws2.Start();


            await ws.Do(StaticHelpers.AuthEvent, communication);
            await ws2.Do(StaticHelpers.AuthEvent, communication);

            await ws.Do(StaticHelpers.EnterRoomEvent, communication);
            await ws2.Do(StaticHelpers.EnterRoomEvent, communication);

            Task.Delay(1000).Wait();
            communication.Should()
                .Contain(x => x.Item1.eventType == nameof(ServerNotifiesClientsInRoomSomeoneHasJoinedRoom));
            communication.Should().NotContain(x => x.Item1.eventType == nameof(ServerSendsErrorMessageToClient));
        }
    }
}