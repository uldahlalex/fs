using api.Models.Enums;
using api.Models.ServerEvents;
using api.StaticHelpers;
using Externalities.QueryModels;
using MediatR;

namespace api.ClientEventHandlers.MediatrInducedEvents;

public class MqttClientWantsToPersistTimeSeriesDataDto : INotification
{
    public TimeSeries TimeSeriesData { get; set; }
}

public class MqttClientWantsToPersistTimeSeriesData : INotificationHandler<MqttClientWantsToPersistTimeSeriesDataDto>
{
    public async Task Handle(MqttClientWantsToPersistTimeSeriesDataDto notification,
        CancellationToken cancellationToken)
    {
        var dto = new ServerBroadcastsTimeSeriesData { timeSeriesDataPoint = notification.TimeSeriesData };

        StaticWebSocketHelpers.BroadcastObjectToTopicListeners(dto, TopicEnums.TimeSeries);
    }
}