using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Formatter;
using MQTTnet.Protocol;
using Serilog;

namespace api;

public class MqttClient
{
    // todo also run with app.Run() or discard app.Run()??
    public async Task Handle_Received_Application_Message()
    {
        var mqttFactory = new MqttFactory();

        using (var mqttClient = mqttFactory.CreateMqttClient())
        {
            var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer("localhost", 1883).WithProtocolVersion(MqttProtocolVersion.V500).Build();

            mqttClient.ApplicationMessageReceivedAsync += async e =>
            {
                Log.Information(e.ApplicationMessage.ConvertPayloadToString());

                // Create the PONG message
               var pongMessage = new MqttApplicationMessageBuilder()
                    .WithTopic("responsetopicedgedevicelistensto")
                    .WithPayload("yes we received the message")
                    .WithQualityOfServiceLevel(e.ApplicationMessage.QualityOfServiceLevel)
                    .WithRetainFlag(e.ApplicationMessage.Retain)
                    .Build();
               await mqttClient.PublishAsync(pongMessage, CancellationToken.None);
            };

            await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

            var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(f => f.WithTopic("mqttnet/samples/topic/1"))
                .Build();

            await mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);
            Console.ReadKey();
        }
    }
}