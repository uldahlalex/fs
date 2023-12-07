namespace core.Models.WebsocketTransferObjects;

public class ServerBroadcastsTimeSeriesData : BaseTransferObject
{
    public TimeSeriesDataPoint? timeSeriesDataPoint { get; set; }
}