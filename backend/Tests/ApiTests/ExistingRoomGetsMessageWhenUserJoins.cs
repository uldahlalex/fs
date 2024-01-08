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
            var communicatoin = new List<Tuple<BaseDto, string>>();

            ws.MessageReceived.Subscribe(msg =>
            {
                communicatoin.Add(
                    new Tuple<BaseDto, string>(msg.Text.DeserializeAndValidate<BaseDto>(), nameof(ws)));
            });
            ws2.MessageReceived.Subscribe(msg =>
            {
                communicatoin.Add(
                    new Tuple<BaseDto, string>(msg.Text.DeserializeAndValidate<BaseDto>(), nameof(ws2)));
            });
            await ws.Start();
            await ws2.Start();


            await ws.Do(StaticHelpers.AuthEvent, communicatoin);
            await ws2.Do(StaticHelpers.AuthEvent, communicatoin);


            await ws.Do(StaticHelpers.EnterRoomEvent, communicatoin);
            await ws2.Do(StaticHelpers.EnterRoomEvent, communicatoin);


            await ws.Do(StaticHelpers.SendMessageEvent, communicatoin);
            await ws2.Do(StaticHelpers.SendMessageEvent, communicatoin);

            communicatoin.Should()
                .Contain(x => x.Item1.eventType == nameof(ServerNotifiesClientsInRoomSomeoneHasJoinedRoom));
            communicatoin.Count(x => x.Item1.eventType == nameof(ServerBroadcastsMessageToClientsInRoom))
                .Should().BeGreaterThan(2);
            communicatoin.Should()
                .NotContain(x => x.Item1.eventType == nameof(ServerSendsErrorMessageToClient));
        }
    }
}