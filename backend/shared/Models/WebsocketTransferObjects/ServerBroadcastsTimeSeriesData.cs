using core.Models.MqttTransferObjects;

namespace core.Models.WebsocketTransferObjects;

public class ServerBroadcastsTimeSeriesData
{
    public TimeSeriesDataPoint? timeSeriesDataPoint { get; set; }
}