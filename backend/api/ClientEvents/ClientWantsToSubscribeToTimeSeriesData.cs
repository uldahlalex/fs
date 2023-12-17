using api.ExtensionMethods;
using api.ServerEvents;
using api.SharedApiModels;
using Infrastructure;
using JetBrains.Annotations;
using MediatR;

namespace api.ClientEvents;

public class ClientWantsToSubscribeToTimeSeriesData : BaseTransferObject
{
}

[UsedImplicitly]
public class ClientWantsToSubscribeToTimeSeriesDataHandler(TimeSeriesRepository timeSeriesRepository)
    : IRequestHandler<EventTypeRequest<ClientWantsToSubscribeToTimeSeriesData>>
{
    public Task Handle(EventTypeRequest<ClientWantsToSubscribeToTimeSeriesData> request,
        CancellationToken cancellationToken)
    {
        request.Socket.SubscribeToTopic("TimeSeries");
        var data = timeSeriesRepository.GetOlderTimeSeriesDataPoints();
        request.Socket.SendDto(new ServerSendsOlderTimeSeriesDataToClient { timeseries = data });
        return Task.CompletedTask;
    }
}