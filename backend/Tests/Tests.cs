using System.Collections.ObjectModel;
using api.ClientEventHandlers;
using api.ExtensionMethods;
using api.Models;
using api.Models.ServerEvents;
using FluentAssertions;
using NUnit.Framework;
using Serilog;
using WebSocketSharp;

namespace Tests;

public class WebsocketServerTests
{
    private readonly Uri _uri = new("ws://localhost:8181");

    public WebsocketServerTests()
    {
        Log.Logger = new LoggerConfiguration() //todo: does serilog also output to Rider test runner?
            .WriteTo.Console(
                outputTemplate: "\n{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}\n")
            .CreateLogger();
    }

    [SetUp]
    public void Setup()
    {
        /*var npg = new NpgsqlDataSourceBuilder(
            Utilities.ProperlyFormattedConnectionString).Build();
        new WebsocketServer(
                new ChatRepository(npg), new TimeSeriesRepository(npg)
                )
            .StartWebsocketServer();*/
        //if you want to run server manually for the tests, simply comment out the lines above, although not recommended
    }

    /**
     * [TestCase(1)] //Tests commented due to need for refactor
     * public async Task ClientCanEnterRoomAndNotifyUsersInRoom(int roomId)
     * {
     * var messagesSentToClient1 = new List
     * <ServerNotifiesClientsInRoom>
     *     ();
     *     var messagesSentToClient2 = new List
     *     <ServerNotifiesClientsInRoom>
     *         ();
     *         var enterRoomEvent = new ClientWantsToEnterRoom
     *         {
     *         roomId = roomId
     *         };
     *         var expectedNotification = new ServerNotifiesClientsInRoomSomeoneHasJoinedRoom()
     *         {
     *         roomId = roomId,
     *         message = "A new user has entered the room!"
     *         //todo add user to the notification?
     *         };
     *         using (var client = new WebsocketClient(_uri))
     *         using (var client2 = new WebsocketClient(_uri))
     *         {
     *         client.MessageReceived.Subscribe(message =>
     *         {
     *         Log.Information("CLIENT 1: GETS: " + message.Text);
     *         if (message.Text.Deserialize
     *         <BaseTransferObject>
     *             ().eventType == nameof(ServerNotifiesClientsInRoom))
     *             messagesSentToClient1.Add(message.Text.Deserialize
     *             <ServerNotifiesClientsInRoom>
     *                 ());
     *                 });
     *                 client2.MessageReceived.Subscribe(message =>
     *                 {
     *                 Log.Information("CLIENT 2: GETS: " + message.Text);
     *                 if (message.Text.Deserialize
     *                 <BaseTransferObject>
     *                     ().eventType == nameof(ServerNotifiesClientsInRoom))
     *                     messagesSentToClient2.Add(message.Text.Deserialize
     *                     <ServerNotifiesClientsInRoom>
     *                         ());
     *                         });
     *                         await client.Start();
     *                         await client2.Start();
     *                         await Task.Delay(TimeSpan.FromSeconds(1));
     *                         client.Send(enterRoomEvent.ToJsonString());
     *                         client2.Send(enterRoomEvent.ToJsonString());
     *                         await Task.Delay(TimeSpan.FromSeconds(1));
     *                         }
     *                         messagesSentToClient1
     *                         .Should()
     *                         .ContainEquivalentOf(expectedNotification);
     *                         messagesSentToClient2
     *                         .Should()
     *                         .ContainEquivalentOf(expectedNotification);
     *                         }
     *                         [TestCase(1, "Hello world!")]
     *                         public async Task ClientCanSendMessageAndServerBroadcastsToRoom(int roomId, string message)
     *                         {
     *                         var messagesSentToClient1 = new List
     *                         <ServerBroadcastsMessageToClientsInRoom>
     *                             ();
     *                             var messagesSentToClient2 = new List
     *                             <ServerBroadcastsMessageToClientsInRoom>
     *                                 ();
     *                                 var enterRoomEvent = new ClientWantsToEnterRoom
     *                                 {
     *                                 roomId = roomId
     *                                 };
     *                                 var sendMessageEvent = new ClientWantsToSendMessageToRoom
     *                                 {
     *                                 roomId = roomId,
     *                                 messageContent = message
     *                                 };
     *                                 using (var client = new WebsocketClient(_uri))
     *                                 using (var client2 = new WebsocketClient(_uri))
     *                                 {
     *                                 client.MessageReceived.Subscribe(message =>
     *                                 {
     *                                 Log.Information("CLIENT 1: GETS: " + message.Text);
     *                                 if (message.Text.Deserialize
     *                                 <BaseTransferObject>
     *                                     ().eventType ==
     *                                     nameof(ServerBroadcastsMessageToClientsInRoom))
     *                                     messagesSentToClient1.Add(message.Text.Deserialize
     *                                     <ServerBroadcastsMessageToClientsInRoom>
     *                                         ());
     *                                         });
     *                                         client2.MessageReceived.Subscribe(message =>
     *                                         {
     *                                         Log.Information("CLIENT 2: GETS: " + message.Text);
     *                                         if (message.Text.Deserialize
     *                                         <BaseTransferObject>
     *                                             ().eventType ==
     *                                             nameof(ServerBroadcastsMessageToClientsInRoom))
     *                                             messagesSentToClient2.Add(message.Text.Deserialize
     *                                             <ServerBroadcastsMessageToClientsInRoom>
     *                                                 ());
     *                                                 });
     *                                                 await client.Start();
     *                                                 await client2.Start();
     *                                                 await Task.Delay(TimeSpan.FromSeconds(1));
     *                                                 client.Send(enterRoomEvent.ToJsonString());
     *                                                 client2.Send(enterRoomEvent.ToJsonString());
     *                                                 client.Send(sendMessageEvent.ToJsonString());
     *                                                 await Task.Delay(TimeSpan.FromSeconds(1));
     *                                                 }
     *                                                 messagesSentToClient1
     *                                                 .First().message.messageContent.Should().Be(message);
     *                                                 messagesSentToClient2
     *                                                 .First().message.messageContent.Should().Be(message);
     *                                                 }
     */
    [Test]
    public async Task Signin_EnterRoom_SendMessage()
    {
        using (var ws = new WebSocket("ws://localhost:8181/"))
        {
            var messagesReceivedFromServer = new List<BaseTransferObject>();
            ws.Connect();
            ws.OnMessage += (sender, e) =>
                messagesReceivedFromServer.Add(e.Data.DeserializeToModelAndValidate<BaseTransferObject>());
            ws.OnError += (sender, e) => { Assert.Fail(); };

            ws.Send(new ClientWantsToAuthenticateDto
            {
                email = "alex@uldahl.dk",
                password = "qweqweqwe"
            }.ToJsonString());
            ws.Send(new ClientWantsToEnterRoomDto
            {
                roomId = 1
            }.ToJsonString());
            ws.Send(new ClientWantsToSendMessageToRoom
            {
                roomId = 1,
                messageContent = "hey"
            }.ToJsonString());

            await Task.Delay(2000);
            Log.Information("Messages: " + messagesReceivedFromServer.ToJsonString());
            messagesReceivedFromServer.Should().Contain(x => x.eventType == nameof(ServerAddsClientToRoom));
            messagesReceivedFromServer.Should().Contain(x => x.eventType == nameof(ServerAuthenticatesUser));
            messagesReceivedFromServer.Should()
                .Contain(x => x.eventType == nameof(ServerBroadcastsMessageToClientsInRoom));
        }
    }

    [Test]
    public async Task Must_Enter_Room_To_Send_Message()
    {
        using (var ws = new WebSocket("ws://localhost:8181/"))
        {
            var messagesReceivedFromServer = new List<BaseTransferObject>();
            ws.Connect();
            ws.OnMessage += (sender, e) =>
                messagesReceivedFromServer.Add(e.Data.DeserializeToModelAndValidate<BaseTransferObject>());
            ws.OnError += (sender, e) => { Assert.Fail(); };

            ws.Send(new ClientWantsToAuthenticateDto
            {
                email = "alex@uldahl.dk",
                password = "qweqweqwe"
            }.ToJsonString());
            ws.Send(new ClientWantsToSendMessageToRoom
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

    [Test]
    public async Task Existing_Clients_In_Room_Get_Joined_Message_When_Someone_Enters()
    {
        using (var ws = new WebSocket("ws://localhost:8181/"))
        using (var ws2 = new WebSocket("ws://localhost:8181/"))
        {
            var messagesReceivedFromServer = new List<BaseTransferObject>();
            ws.Connect();
            ws2.Connect();
            ws.OnMessage += (sender, e) =>
                messagesReceivedFromServer.Add(e.Data.DeserializeToModelAndValidate<BaseTransferObject>());
            ws.OnError += (sender, e) => { Assert.Fail(); };

            ws2.OnMessage += (sender, e) =>
                messagesReceivedFromServer.Add(e.Data.DeserializeToModelAndValidate<BaseTransferObject>());
            ws2.OnError += (sender, e) => { Assert.Fail(); };
            var auth = new ClientWantsToAuthenticateDto
            {
                email = "alex@uldahl.dk",
                password = "qweqweqwe"
            }.ToJsonString();
            ws.Send(auth);
            ws2.Send(auth);
            var enterRoom = new ClientWantsToEnterRoomDto
            {
                roomId = 1
            }.ToJsonString();
            ws.Send(enterRoom);
            await Task.Delay(2000);
            ws2.Send(enterRoom);


            await Task.Delay(2000);
            Log.Information("Messages: " + messagesReceivedFromServer.ToJsonString());
            messagesReceivedFromServer.Should()
                .Contain(x => x.eventType == nameof(ServerNotifiesClientsInRoomSomeoneHasJoinedRoom));
            messagesReceivedFromServer.Should().NotContain(x => x.eventType == nameof(ServerSendsErrorMessageToClient));
        }
    }

    [Test]
    public async Task Message_Broadcasting_In_Room_works()
    {
        using (var ws = new WebSocket("ws://localhost:8181/"))
        using (var ws2 = new WebSocket("ws://localhost:8181/"))
        {
            var messagesReceivedFromServer = new Collection<BaseTransferObject>();
            ws.Connect();
            ws2.Connect();
            ws.OnMessage += (sender, e) =>
                messagesReceivedFromServer.Add(e.Data.DeserializeToModelAndValidate<BaseTransferObject>());
            ws.OnError += (sender, e) => { Assert.Fail(); };

            ws2.OnMessage += (sender, e) =>
                messagesReceivedFromServer.Add(e.Data.DeserializeToModelAndValidate<BaseTransferObject>());
            ws2.OnError += (sender, e) => { Assert.Fail(); };
            var auth = new ClientWantsToAuthenticateDto
            {
                email = "alex@uldahl.dk",
                password = "qweqweqwe"
            }.ToJsonString();
            ws.Send(auth);
            ws2.Send(auth);
            var enterRoom = new ClientWantsToEnterRoomDto
            {
                roomId = 1
            }.ToJsonString();
            ws.Send(enterRoom);
            ws2.Send(enterRoom);
            var message = new ClientWantsToSendMessageToRoomDto
            {
                roomId = 1,
                messageContent = "hey"
            }.ToJsonString();
            ws.Send(message);

            await Task.Delay(2000);
            Log.Information("Messages: " + messagesReceivedFromServer.ToJsonString());
            messagesReceivedFromServer.Should()
                .Contain(x => x.eventType == nameof(ServerNotifiesClientsInRoomSomeoneHasJoinedRoom));
            messagesReceivedFromServer.Count(x => x.eventType == nameof(ServerBroadcastsMessageToClientsInRoom))
                .Should().Be(2);
            messagesReceivedFromServer.Should().NotContain(x => x.eventType == nameof(ServerSendsErrorMessageToClient));
        }
    }
}