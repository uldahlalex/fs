using api.Attributes.EventFilters;
using api.Models.Enums;
using api.Models.ServerEvents;
using api.StaticHelpers.ExtensionMethods;
using Externalities;
using Fleck;
using lib;

namespace api.ClientEventHandlers;

[RequireAuthentication]
public class ClientWantsToSubscribeToTimeSeriesDataDto : BaseDto;

[RateLimit(5, 60)]
public class ClientWantsToSubscribeToTimeSeriesData(TimeSeriesRepository timeSeriesRepository)
    : BaseEventHandler<ClientWantsToSubscribeToTimeSeriesDataDto>
{
    public override Task Handle(ClientWantsToSubscribeToTimeSeriesDataDto dto, IWebSocketConnection socket)
    {
        socket.SubscribeToTopic(TopicEnums.TimeSeries);
        var data = timeSeriesRepository.GetOlderTimeSeriesDataPoints();
        socket.SendDto(new ServerSendsOlderTimeSeriesDataToClient { timeseries = data });
        return Task.CompletedTask;
    }
}