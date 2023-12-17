using api.SharedApiModels;
using core.Models.DbModels;

namespace api.ServerEvents;

public class ServerBroadcastsTimeSeriesData : BaseTransferObject
{
    public TimeSeries? timeSeriesDataPoint { get; set; }
}