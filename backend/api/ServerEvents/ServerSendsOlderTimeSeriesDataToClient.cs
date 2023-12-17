using api.SharedApiModels;
using core.Models.DbModels;

namespace api.ServerEvents;

public class ServerSendsOlderTimeSeriesDataToClient : BaseTransferObject
{
    public IEnumerable<TimeSeries> timeseries { get; set; }
}