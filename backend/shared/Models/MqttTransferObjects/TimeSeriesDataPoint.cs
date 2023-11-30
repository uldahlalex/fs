namespace core.Models.MqttTransferObjects;

public class TimeSeriesDataPoint
{
    public int id { get; set; }
    public string? messageContent { get; set; }
    public DateTimeOffset timestamp { get; set; }
}