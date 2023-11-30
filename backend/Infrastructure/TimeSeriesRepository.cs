using core.Models;
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
drop schema demo cascade;
CREATE SCHEMA IF NOT EXISTS demo;
CREATE TABLE IF NOT EXISTS demo.timeseries
(id int primary key generated always as identity,
 messageContent TEXT, timestamp TIMESTAMP WITH TIME ZONE);

");
        }
    }

    public TimeSeriesDataPoint PersistTimeSeriesDataPoint(TimeSeriesDataPoint dataPoint)
    {
        var sql = $@"INSERT INTO demo.timeseries (messageContent, timestamp) VALUES (@messageContent, @timestamp) RETURNING *;";

        using (var conn = _dataSource.OpenConnection())
        {
            return conn.QueryFirst<TimeSeriesDataPoint>(sql, new {messageContent = dataPoint.messageContent, timestamp = dataPoint.timestamp});
        }
    }
}

