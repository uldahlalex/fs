using Infrastructure.DbModels;

namespace api.Models.ServerEvents;

public class ServerSendsOlderTimeSeriesDataToClient : BaseTransferObject
{
    public IEnumerable<TimeSeries> timeseries { get; set; }
}