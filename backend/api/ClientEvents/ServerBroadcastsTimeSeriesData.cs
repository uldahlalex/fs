using api;
using core.Models.DbModels;

namespace core.Models.WebsocketTransferObjects;

public class ServerBroadcastsTimeSeriesData : BaseTransferObject
{
    public TimeSeries? timeSeriesDataPoint { get; set; }
}