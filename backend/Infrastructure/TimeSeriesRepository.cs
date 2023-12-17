using Dapper;
using Infrastructure.DbModels;
using Npgsql;

namespace Infrastructure;

public class TimeSeriesRepository(NpgsqlDataSource dataSource)
{
    public TimeSeries PersistTimeSeriesDataPoint(TimeSeries timeseries)
    {
        var sql =
            @"INSERT INTO demo.timeseries (datapoint, timestamp) VALUES (@datapoint, @timestamp) RETURNING *;";

        using (var conn = dataSource.OpenConnection())
        {
            return conn.QueryFirst<TimeSeries>(sql,
                new { timeseries.datapoint, timestamp = DateTimeOffset.UtcNow });
        }
    }

    public IEnumerable<TimeSeries> GetOlderTimeSeriesDataPoints()
    {
        var sql = "SELECT * FROM demo.timeseries LIMIT 100;";
        using (var conn = dataSource.OpenConnection())
        {
            return conn.Query<TimeSeries>(sql);
        }
    }
}