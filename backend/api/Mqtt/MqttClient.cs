using api.Websocket;
using core.Models.MqttTransferObjects;
using core.Models.WebsocketTransferObjects;
using core.TextTools;
using Infrastructure;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Formatter;
using Newtonsoft.Json;
using Serilog;

namespace api.Mqtt;

public class MqttClient(TimeSeriesRepository timeSeriesRepository, WebsocketServer websocketServer)
{
    public async Task Handle_Received_Application_Message()
    {
        var mqttFactory = new MqttFactory();
        var mqttClient = mqttFactory.CreateMqttClient();

        var mqttClientOptions = new MqttClientOptionsBuilder()
            .WithTcpServer("localhost", 1883)
            .WithProtocolVersion(MqttProtocolVersion.V500)
            .Build();

        mqttClient.ApplicationMessageReceivedAsync += async e =>
        {
            var message = e.ApplicationMessage.ConvertPayloadToString();
            Log.Information(message);
            var timeSeriesDataPoint = Deserializer<TimeSeriesDataPoint>.Deserialize(message);
            timeSeriesDataPoint.timestamp = DateTimeOffset.UtcNow;
            var insertionResult = timeSeriesRepository.PersistTimeSeriesDataPoint(timeSeriesDataPoint);
            var serializedTimeSeries = JsonConvert.SerializeObject( 
                new ServerBroadcastsTimeSeriesData { timeSeriesDataPoint = insertionResult });
            
            foreach (var websocketServerLiveSocketConnection in websocketServer.LiveSocketConnections)
            {
                await websocketServerLiveSocketConnection.Value.Send(serializedTimeSeries);
            }
            
            var pongMessage = new MqttApplicationMessageBuilder()
                .WithTopic("response_topic")
                .WithPayload("yes we received the message, thank you very much, the websocket client also has the data")
                .WithQualityOfServiceLevel(e.ApplicationMessage.QualityOfServiceLevel)
                .WithRetainFlag(e.ApplicationMessage.Retain)
                .Build();
            await mqttClient.PublishAsync(pongMessage, CancellationToken.None);
        };

        await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

        var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
            .WithTopicFilter(f => f.WithTopic("timeseries"))
            .Build();

        await mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);
    }
}