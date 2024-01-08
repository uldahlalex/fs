using System.Collections.ObjectModel;
using api.ClientEventHandlers;
using api.Extensions;
using api.Models;
using api.Models.ServerEvents;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using Serilog;
using Websocket.Client;
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
        using (var ws = new WebsocketClient(new Uri(StaticHelpers.Url)))
        using (var ws2 = new WebsocketClient(new Uri(StaticHelpers.Url)))
        {
            var messagesReceivedFromServer = new List<Tuple<BaseDto, string>>();
     

            // Subscribe to message and error events
            
            ws.MessageReceived.Subscribe(msg =>
            {
                messagesReceivedFromServer.Add(new Tuple<BaseDto, string>(msg.Text.DeserializeAndValidate<BaseDto>(), nameof(ws)));
            });
            ws2.MessageReceived.Subscribe(msg =>
            {
                messagesReceivedFromServer.Add(new Tuple<BaseDto, string>(msg.Text.DeserializeAndValidate<BaseDto>(), nameof(ws2)));
            });
            await ws.Start();
            await ws2.Start();

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
            await ws2.Do(message, messagesReceivedFromServer);
            
            Console.WriteLine("Messages: " + messagesReceivedFromServer.ToJsonString());
            messagesReceivedFromServer.Should()
                .Contain(x => x.Item1.eventType == nameof(ServerNotifiesClientsInRoomSomeoneHasJoinedRoom));
            messagesReceivedFromServer.Count(x => x.Item1.eventType == nameof(ServerBroadcastsMessageToClientsInRoom))
                .Should().BeGreaterThan(2);
            messagesReceivedFromServer.Should()
                .NotContain(x => x.Item1.eventType == nameof(ServerSendsErrorMessageToClient));
        }
    }
}