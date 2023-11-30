using core.Models.MqttTransferObjects;
using Dapper;
using Npgsql;

namespace Infrastructure;

public class TimeSeriesRepository
{
    private readonly NpgsqlDataSource _dataSource;
    
    public TimeSeriesRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
        using (var conn = _dataSource.OpenConnection())
        {
            conn.Execute(@"
CREATE SCHEMA IF NOT EXISTS demo;
CREATE TABLE IF NOT EXISTS demo.timeseries
(id SERIAL PRIMARY KEY, messageContent TEXT, timestamp TIMESTAMP);
");
        }
    }

    public TimeSeriesDataPoint PersistTimeSeriesDataPoint(TimeSeriesDataPoint dataPoint)
    {
        var sql = $@"INSERT INTO demo.timeseries (messageContent, timestamp) RETURNING *;";

        using (var conn = _dataSource.OpenConnection())
        {
            return conn.QueryFirst<TimeSeriesDataPoint>(sql, dataPoint);
        }
    }
}

