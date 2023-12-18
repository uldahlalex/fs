using api.ExtensionMethods;
using api.Helpers;
using api.Models.ServerEvents;
using Infrastructure;
using Infrastructure.DbModels;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Formatter;
using Serilog;

namespace api;

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

        await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

        var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
            .WithTopicFilter(f => f.WithTopic("TimeSeries"))
            .Build();

        await mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);

        mqttClient.ApplicationMessageReceivedAsync += async e =>
        {
            try
            {
                var message = e.ApplicationMessage.ConvertPayloadToString();
                Log.Information(message);
                var timeSeriesDataPoint = message.Deserialize<TimeSeries>();
                timeSeriesDataPoint.timestamp = DateTimeOffset.UtcNow;
                var insertionResult = timeSeriesRepository.PersistTimeSeriesDataPoint(timeSeriesDataPoint);
                var dto = new ServerBroadcastsTimeSeriesData { timeSeriesDataPoint = insertionResult };

                SocketUtilities.BroadcastObjectToTopicListeners(dto, "TimeSeries");

                var pongMessage = new MqttApplicationMessageBuilder()
                    .WithTopic("response_topic")
                    .WithPayload("yes we received the message, thank you very much, " +
                                 "the websocket client(s) also has the data")
                    .WithQualityOfServiceLevel(e.ApplicationMessage.QualityOfServiceLevel)
                    .WithRetainFlag(e.ApplicationMessage.Retain)
                    .Build();
                await mqttClient.PublishAsync(pongMessage, CancellationToken.None);
            }
            catch (Exception exc)
            {
                Log.Logger.Error(exc, "MqttClient");
            }
        };
    }
}