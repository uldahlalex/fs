using api.SharedApiModels;
using Infrastructure.DbModels;

namespace api.ServerEvents;

public class ServerBroadcastsTimeSeriesData : BaseTransferObject
{
    public TimeSeries? timeSeriesDataPoint { get; set; }
}