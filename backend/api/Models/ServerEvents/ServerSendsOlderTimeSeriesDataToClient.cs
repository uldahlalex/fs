using Externalities.QueryModels;

namespace api.Models.ServerEvents;

public class ServerSendsOlderTimeSeriesDataToClient : BaseDto
{
    public IEnumerable<TimeSeries> timeseries { get; set; }
}