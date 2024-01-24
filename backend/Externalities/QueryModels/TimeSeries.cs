namespace Externalities.QueryModels;

public class TimeSeries
{
    public int id { get; set; }
    public int? datapoint { get; set; }
    public DateTimeOffset timestamp { get; set; }
}