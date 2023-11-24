using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Formatter;
using Serilog;

namespace api.Mqtt;

public class MqttClient
{


    public async Task Handle_Received_Application_Message()
    {
        var mqttFactory = new MqttFactory();

        var mqttClient = mqttFactory.CreateMqttClient();
        
            var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer("localhost", 1883).WithProtocolVersion(MqttProtocolVersion.V500).Build();

            mqttClient.ApplicationMessageReceivedAsync += async e =>
            {
                var message = e.ApplicationMessage.ConvertPayloadToString();
                Log.Information(message);
                
                //persist and pass on to websocket broadcaster

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
        
    }
}