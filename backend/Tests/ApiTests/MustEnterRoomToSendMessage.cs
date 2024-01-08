using api.ClientEventHandlers;
using api.Extensions;
using api.Models;
using api.Models.ServerEvents;
using FluentAssertions;
using NUnit.Framework;
using Serilog;
using WebSocketSharp;

namespace Tests.ApiTests;

public class MustEnterRoomToSendMessage
{
    [Test]
    public async Task Must_Enter_Room_To_Send_Message()
    {
        using (var ws = new WebSocket(StaticHelpers.Url))
        {
            var messagesReceivedFromServer = new List<BaseDto>();
            ws.Connect();
            ws.OnMessage += (sender, e) =>
                messagesReceivedFromServer.Add(e.Data.DeserializeAndValidate<BaseDto>());
            ws.OnError += (sender, e) => { Assert.Fail(); };

            ws.Send(new ClientWantsToAuthenticateDto
            {
                email = "bla@bla.dk",
                password = "qweqweqwe"
            }.ToJsonString());
            ws.Send(new ClientWantsToSendMessageToRoomDto
            {
                roomId = 1,
                messageContent = "hey"
            }.ToJsonString());


            await Task.Delay(2000);
            Log.Information("Messages: " + messagesReceivedFromServer.ToJsonString());
            messagesReceivedFromServer.Should().Contain(x => x.eventType == nameof(ServerAuthenticatesUser));
            messagesReceivedFromServer.Should().Contain(x => x.eventType == nameof(ServerSendsErrorMessageToClient));
            messagesReceivedFromServer.Should().NotContain(x => x.eventType == nameof(ServerAddsClientToRoom));
        }
    }
}