using api.Models.DbModels;
using api.Models.Enums;
using Commons;
using MediatR;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Formatter;

namespace api.Externalities;

public class MqttClient(TimeSeriesRepository timeSeriesRepository, IMediator mediator)
{
    public async Task Handle_Received_Application_Message()
    {
        var mqttFactory = new MqttFactory();
        var mqttClient = mqttFactory.CreateMqttClient();

        var isDocker = File.Exists("/.dockerenv");
        var mqttServer = isDocker ? "mqtt-broker" : "localhost";
        var mqttClientOptions = new MqttClientOptionsBuilder()
            .WithTcpServer(mqttServer, 1883)
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
                var ts = message.DeserializeAndValidate<TimeSeries>();
                ts.timestamp = DateTimeOffset.UtcNow;
                var insertionResult = timeSeriesRepository.PersistTimeSeriesDataPoint(ts);
                await mediator.Publish(new MqttClientWantsToPersistTimeSeriesDataDto()
                {
                    TimeSeriesData = insertionResult
                });

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
                Console.WriteLine(exc.Message);
                Console.WriteLine(exc.InnerException);
                Console.WriteLine(exc.StackTrace);
            }
        };
    }
}

public class MqttClientWantsToPersistTimeSeriesDataDto : INotification
{
    public TimeSeries TimeSeriesData { get; set; }
}