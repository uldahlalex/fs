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

public class ExistingRoomGetsMessageWhenUserJoins
{
    [SetUp]
    public void Setup()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(
                outputTemplate: "\n{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}\n")
            .CreateLogger();
    }

    [Test]
    public async Task Message_Broadcasting_In_Room_works()
    {
        using (var ws = new WebSocket(StaticHelpers.Url))
        using (var ws2 = new WebSocket(StaticHelpers.Url))
        {
            var messagesReceivedFromServer = new List<Tuple<BaseDto, string>>();
            ws.Connect();
            ws2.Connect();
            ws.OnMessage += (sender, e) =>
                messagesReceivedFromServer.Add(new Tuple<BaseDto, string>(e.Data.DeserializeAndValidate<BaseDto>(),
                    nameof(ws)));
            ws.OnError += (sender, e) => { Assert.Fail(); };

            ws2.OnMessage += (sender, e) =>
                messagesReceivedFromServer.Add(new Tuple<BaseDto, string>(e.Data.DeserializeAndValidate<BaseDto>(),
                    nameof(ws2)));
            ws2.OnError += (sender, e) => { Assert.Fail(); };


            var auth = new ClientWantsToAuthenticateDto
            {
                email = "bla@bla.dk",
                password = "qweqweqwe"
            };
            await ws.Do(auth, messagesReceivedFromServer);
            await ws2.Do(auth, messagesReceivedFromServer);
            
            var enterRoom = new ClientWantsToEnterRoomDto
            {
                roomId = 1
            };
            await ws.Do(enterRoom, messagesReceivedFromServer);
            await ws2.Do(enterRoom, messagesReceivedFromServer);

            var message = new ClientWantsToSendMessageToRoomDto
            {
                roomId = 1,
                messageContent = "hey"
            };
            await ws.Do(message, messagesReceivedFromServer);

            Console.WriteLine("Messages: " + messagesReceivedFromServer.ToJsonString());
            messagesReceivedFromServer.Should()
                .Contain(x => x.Item1.eventType == nameof(ServerNotifiesClientsInRoomSomeoneHasJoinedRoom));
            messagesReceivedFromServer.Count(x => x.Item1.eventType == nameof(ServerBroadcastsMessageToClientsInRoom))
                .Should().Be(2);
            messagesReceivedFromServer.Should()
                .NotContain(x => x.Item1.eventType == nameof(ServerSendsErrorMessageToClient));
        }
    }
}