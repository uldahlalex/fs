using System.Text.Json;
using api.Models;
using Commons;
using Websocket.Client;

public class WebSocketTestClient
{
    public readonly WebsocketClient Client;
    private readonly Dictionary<string, TaskCompletionSource<BaseDto>> _waitingTasks = new();
    private readonly Dictionary<string, List<BaseDto>> _messageBuffer = new();

    public WebSocketTestClient()
    {
        Client = new WebsocketClient(new Uri("ws://localhost:" +
                                             Environment.GetEnvironmentVariable("FULLSTACK_API_PORT")));
        Client.MessageReceived.Subscribe(OnMessageReceived);
    }


    public async Task<List<BaseDto>>  DoAndWaitUntil<T>(T action, List<string> expectedEvents,  TimeSpan? timeout = null) where T : BaseDto
    {
        // Send the action
        Send(action);
        var responses = new List<BaseDto>();

        foreach (var eventType in expectedEvents)
        {
            var response = await WaitForEventAsync(eventType);
            responses.Add(response);
        }
        return responses;
    }

    private void OnMessageReceived(ResponseMessage message)
    {
        var eventDto = message.Text.Deserialize<BaseDto>();
        var eventType = eventDto.eventType;

        // Check if there's a waiting task for this event
        if (_waitingTasks.TryGetValue(eventType, out var tcs))
        {
            tcs.SetResult(eventDto);
            _waitingTasks.Remove(eventType);
        }
        else
        {
            // If no task is waiting, buffer the message
            if (!_messageBuffer.ContainsKey(eventType))
                _messageBuffer[eventType] = new List<BaseDto>();

            _messageBuffer[eventType].Add(eventDto);
        }
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

    public Task<BaseDto> WaitForEventAsync(string eventType)
    {
        // Check if the event is already in the buffer
        if (_messageBuffer.TryGetValue(eventType, out var messages) && messages.Any())
        {
            var message = messages.First();
            messages.RemoveAt(0); // Remove the message from the buffer
            return Task.FromResult(message);
        }

        var tcs = new TaskCompletionSource<BaseDto>();

        {
            var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(2)).Token;
            cancellationToken.Register(() => { tcs.TrySetCanceled(); });


            _waitingTasks[eventType] = tcs;
            return tcs.Task;
        }
    }
}