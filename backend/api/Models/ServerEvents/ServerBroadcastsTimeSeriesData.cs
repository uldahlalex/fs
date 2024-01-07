using api.Models.DbModels;

namespace api.Models.ServerEvents;

public class ServerBroadcastsTimeSeriesData : BaseDto
{
    public TimeSeries? timeSeriesDataPoint { get; set; }
}