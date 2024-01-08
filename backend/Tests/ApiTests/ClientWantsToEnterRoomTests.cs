using api.Abstractions;
using api.ClientEventHandlers;
using api.Extensions;
using api.Models;
using api.Models.ServerEvents;
using FluentAssertions;
using NUnit.Framework;
using Serilog;
using Websocket.Client;
using Xunit;

namespace Tests.ApiTests;

[TestFixture]
public class MustEnterRoomToSendMessage
{
    
    [Test]
    public async Task Must_Enter_Room_To_Send_Message()
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
            await ws.Do(StaticHelpers.SendMessageEvent, communication);
            
            communication.Should().Contain(x => x.Item1.eventType == nameof(ServerAuthenticatesUser));
            communication.Should().Contain(x => x.Item1.eventType == nameof(ServerSendsErrorMessageToClient));
            communication.Should().NotContain(x => x.Item1.eventType == nameof(ServerAddsClientToRoom));
        }
    }
}