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
            var communication = new List<Tuple<BaseDto, string>>();

            ws.MessageReceived.Subscribe(msg =>
            {
                communication.Add(
                    new Tuple<BaseDto, string>(msg.Text.DeserializeAndValidate<BaseDto>(), nameof(ws)));
            });

            await ws.Start();
            await ws.Do(StaticHelpers.AuthEvent, communication);
            await ws.Do(StaticHelpers.EnterRoomEvent, communication);
            await ws.Do(StaticHelpers.SendMessageEvent, communication);


            Task.Delay(1000).Wait();
            communication.Should().Contain(x => x.Item1.eventType == nameof(ServerAddsClientToRoom));
            communication.Should().Contain(x => x.Item1.eventType == nameof(ServerAuthenticatesUser));
            communication.Should()
                .Contain(x => x.Item1.eventType == nameof(ServerBroadcastsMessageToClientsInRoom));
        }
    }
}