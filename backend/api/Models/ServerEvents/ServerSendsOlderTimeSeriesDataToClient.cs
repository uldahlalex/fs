using Externalities.QueryModels;
using lib;

namespace api.Models.ServerEvents;

public class ServerSendsOlderTimeSeriesDataToClient : BaseDto
{
    public IEnumerable<TimeSeries> timeseries { get; set; }
}