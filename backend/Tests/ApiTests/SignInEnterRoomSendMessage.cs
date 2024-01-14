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
            var communication = new List<(BaseDto dto, string websocketClient)>();

            ws.MessageReceived.Subscribe(msg =>
            {
                communication.Add(
                    new ValueTuple<BaseDto, string>(msg.Text.DeserializeAndValidate<BaseDto>(), nameof(ws)));
            });

            await ws.Start();

    
            await ws.Do(StaticHelpers.AuthEvent, communication, () => communication.Any(x => x.dto.eventType == nameof(ServerAuthenticatesUser)));

            await ws.Do(StaticHelpers.EnterRoomEvent, communication, () => communication.Any(x => x.dto.eventType == nameof(ServerAddsClientToRoom)));

            await ws.Do(StaticHelpers.SendMessageEvent, communication, () => communication.Any(x => x.dto.eventType == nameof(ServerBroadcastsMessageToClientsInRoom))); 

            communication.Should().Contain(x => x.Item1.eventType == nameof(ServerAddsClientToRoom));
            communication.Should().Contain(x => x.Item1.eventType == nameof(ServerAuthenticatesUser));
            communication.Should().Contain(x => x.Item1.eventType == nameof(ServerBroadcastsMessageToClientsInRoom));
        }
    }
}