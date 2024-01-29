using System.Text.Json;
using api.Models;
using Websocket.Client;

namespace Tests;

public class WebSocketTestClient
{
    public readonly WebsocketClient Client;
    public readonly List<BaseDto> ReceivedMessages = new();

    public WebSocketTestClient()
    {
        Client = new WebsocketClient(new Uri("ws://localhost:"+Environment.GetEnvironmentVariable("FULLSTACK_API_PORT")));
        Client.MessageReceived.Subscribe(msg => 
        {
            var dto = JsonSerializer.Deserialize<BaseDto>(msg.Text); // Adjust deserialization as needed
            lock (ReceivedMessages)
            {
                ReceivedMessages.Add(dto);
            }
        });
    }

    public async Task<WebSocketTestClient> ConnectAsync()
    {
        await Client.Start();
        if (!Client.IsRunning) throw new Exception("Could not start client!");
        return this;
    }

    public void Send<T>(T dto) where T : BaseDto
    {
        var serialized = JsonSerializer.Serialize(dto);
        Client.Send(serialized);
    }

    public async Task DoAndAssert<T>(T? action = null, Func<List<BaseDto>, bool>? condition = null) where T : BaseDto
    {
        if(action != null)
            Send(action);

        if(condition == null)
            return;
        var startTime = DateTime.UtcNow;
        while (DateTime.UtcNow - startTime < TimeSpan.FromSeconds(5))
        {
            lock (ReceivedMessages)
            {
                if (condition(ReceivedMessages))
                {
                    return; 
                }
            }

            await Task.Delay(100); 
        }

        throw new TimeoutException($"Condition not met: ");
    }
    

}