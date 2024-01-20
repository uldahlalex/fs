namespace api.Models.DbModels;

public class TimeSeries
{
    public int id { get; set; }
    public int? datapoint { get; set; }
    public DateTimeOffset timestamp { get; set; }
}