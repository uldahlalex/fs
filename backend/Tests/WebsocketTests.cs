using System.Net.Http.Headers;
using core.Models;
using FluentAssertions;
using NUnit.Framework;
using Websocket.Client;
using JsonSerializer = System.Text.Json.JsonSerializer;
using JsonSerializerOptions = System.Text.Json.JsonSerializerOptions;

namespace Tests;

public class Tests
{
    //todo rewrite

    private const string WebSocketServerUri = "ws://localhost:8181/1";


    [TestCase("Hello, server!")]
    public async Task TwoClientsCanConnectAndSendMessage(string message)
    {
        var receivedResponses = new List<List<Message>>();
        using (var client = new WebsocketClient(new Uri(WebSocketServerUri)))
        {
            client.MessageReceived.Subscribe(msg =>
            {
                List<Message> response = JsonSerializer.Deserialize<List<Message>>(
                                             msg.Text ?? throw new InvalidOperationException(
                                                 "Could not deserialize into a List<Message>"),
                                             new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                                         ?? throw new ArgumentException("Could not convert to MessageDto");
                receivedResponses.Add(response);
            });
            await client.Start();

            client.Send(message);

            await Task.Delay(TimeSpan.FromSeconds(1));
            receivedResponses[0][1].messageContent.Should().Be(message);
        }
    }

    public void test()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", Environment.GetEnvironmentVariable("token"));
    }
}