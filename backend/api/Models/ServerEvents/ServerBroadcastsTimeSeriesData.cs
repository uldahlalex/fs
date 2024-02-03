using Externalities.QueryModels;
using lib;

namespace api.Models.ServerEvents;

public class ServerBroadcastsTimeSeriesData : BaseDto
{
    public TimeSeries? timeSeriesDataPoint { get; set; }
}