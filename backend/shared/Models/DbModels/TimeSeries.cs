using core.Attributes;

namespace core.Models.DbModels;

public class TimeSeries
{
    [EnforceName("id")] public int id { get; set; }

    [EnforceName("datapoint")] public int? datapoint { get; set; }

    [EnforceName("timestamp")] public DateTimeOffset timestamp { get; set; }
}