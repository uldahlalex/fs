using api.SharedApiModels;
using Infrastructure.DbModels;

namespace api.ServerEvents;

public class ServerSendsOlderTimeSeriesDataToClient : BaseTransferObject
{
    public IEnumerable<TimeSeries> timeseries { get; set; }
}