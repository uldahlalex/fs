using api.Abstractions;
using api.ExtensionMethods;
using api.Models;
using api.Models.Enums;
using api.Models.ServerEvents;
using Fleck;
using Infrastructure;

namespace api.ClientEventHandlers;

public class ClientWantsToSubscribeToTimeSeriesDataDto : BaseTransferObject;

public class ClientWantsToSubscribeToTimeSeriesData(TimeSeriesRepository timeSeriesRepository) : BaseEventHandler<ClientWantsToSubscribeToTimeSeriesDataDto>
{
    public override Task Handle(ClientWantsToSubscribeToTimeSeriesDataDto dto, IWebSocketConnection socket)
    {
        socket.SubscribeToTopic(TopicEnums.TimeSeries);
        var data = timeSeriesRepository.GetOlderTimeSeriesDataPoints();
        socket.SendDto(new ServerSendsOlderTimeSeriesDataToClient { timeseries = data });
        return Task.CompletedTask;
    }
}