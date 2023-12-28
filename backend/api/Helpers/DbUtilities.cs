﻿namespace api.Helpers;

public class Utilities
{
    private static readonly Uri Uri = new(Environment.GetEnvironmentVariable("FULLSTACK_PG_CONN")!);

    public static readonly string
        ProperlyFormattedConnectionString = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Docker"
            ? "Server=db;Database=postgres;User Id=postgres;Password=postgres;Port=5432;Pooling=true;MaxPoolSize=3"
            : string.Format(
                "Server={0};Database={1};User Id={2};Password={3};Port={4};Pooling=true;MaxPoolSize=3",
                Uri.Host,
                Uri.AbsolutePath.Trim('/'),
                Uri.UserInfo.Split(':')[0],
                Uri.UserInfo.Split(':')[1],
                Uri.Port > 0 ? Uri.Port : 5432);
}