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
            var communication = new List<(BaseDto dto, string websocketClient)>();

            ws.MessageReceived.Subscribe(msg =>
            {
                communication.Add(
                    new ValueTuple<BaseDto, string>(msg.Text.DeserializeAndValidate<BaseDto>(), nameof(ws)));
            });
            ws2.MessageReceived.Subscribe(msg =>
            {
                communication.Add(
                    new ValueTuple<BaseDto, string>(msg.Text.DeserializeAndValidate<BaseDto>(), nameof(ws)));
            });
            await ws.Start();
            await ws2.Start();


            await ws.Do(StaticHelpers.AuthEvent, communication, () => communication.Any(x => x.Item1.eventType == nameof(ServerAuthenticatesUser)));
            await ws2.Do(StaticHelpers.AuthEvent, communication, () => communication.Count(x => x.Item1.eventType == nameof(ServerAuthenticatesUser))==2);


            await ws.Do(StaticHelpers.EnterRoomEvent, communication, () => communication.Any(x => x.Item1.eventType == nameof(ServerAddsClientToRoom)));
            await ws2.Do(StaticHelpers.EnterRoomEvent, communication,
                () => communication.Count(x => x.Item1.eventType == nameof(ServerAddsClientToRoom))==2);

            await ws.Do(StaticHelpers.SendMessageEvent, communication,
                () => communication.Any(x => x.Item1.eventType == nameof(ServerBroadcastsMessageToClientsInRoom)));
            await ws2.Do(StaticHelpers.SendMessageEvent, communication,
                () => communication.Count(x => x.Item1.eventType == nameof(ServerBroadcastsMessageToClientsInRoom)) >= 3);
            
            communication.Should()
                .Contain(x => x.Item1.eventType == nameof(ServerNotifiesClientsInRoomSomeoneHasJoinedRoom));
            communication.Count(x => x.Item1.eventType == nameof(ServerBroadcastsMessageToClientsInRoom))
                .Should().BeGreaterThanOrEqualTo(3);
            communication.Should()
                .NotContain(x => x.Item1.eventType == nameof(ServerSendsErrorMessageToClient));
        }
    }
}