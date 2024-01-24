using Dapper;
using Externalities.QueryModels;
using Npgsql;

namespace Externalities;

public class TimeSeriesRepository(NpgsqlDataSource dataSource)
{
    public TimeSeries PersistTimeSeriesDataPoint(TimeSeries timeseries)
    {
        var sql =
            @"INSERT INTO chat.timeseries (datapoint, timestamp) VALUES (@datapoint, @timestamp) RETURNING *;";

        using (var conn = dataSource.OpenConnection())
        {
            return conn.QueryFirst<TimeSeries>(sql,
                new { timeseries.datapoint, timestamp = DateTimeOffset.UtcNow });
        }
    }

    public IEnumerable<TimeSeries> GetOlderTimeSeriesDataPoints()
    {
        var sql = "SELECT * FROM chat.timeseries LIMIT 100;";
        using (var conn = dataSource.OpenConnection())
        {
            return conn.Query<TimeSeries>(sql);
        }
    }
}