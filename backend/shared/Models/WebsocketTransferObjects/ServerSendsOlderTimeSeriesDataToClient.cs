using core.Models.DbModels;

namespace core.Models.WebsocketTransferObjects;

public class ServerSendsOlderTimeSeriesDataToClient : BaseTransferObject
{
    public IEnumerable<TimeSeries> timeseries { get; set; }
}