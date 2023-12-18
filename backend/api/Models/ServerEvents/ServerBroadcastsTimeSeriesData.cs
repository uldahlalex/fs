using Infrastructure.DbModels;

namespace api.Models.ServerEvents;

public class ServerBroadcastsTimeSeriesData : BaseTransferObject
{
    public TimeSeries? timeSeriesDataPoint { get; set; }
}