using Dapper;
using Npgsql;

namespace Externalities;

public static class Utilities
{
    private static readonly Uri Uri = new(Environment.GetEnvironmentVariable("FULLSTACK_PG_CONN_PRODUCTION")!);

    public static readonly string
        ProperlyFormattedConnectionString = string.Format(
            "Server={0};Database={1};User Id={2};Password={3};Port={4};Pooling=true;MaxPoolSize=3",
            Uri.Host,
            Uri.AbsolutePath.Trim('/'),
            Uri.UserInfo.Split(':')[0],
            Uri.UserInfo.Split(':')[1],
            Uri.Port > 0 ? Uri.Port : 5432);

    public static void ExecuteRebuildFromSqlScript(string? alternativeConnectionString = null)
    {
        using (var conn = new NpgsqlConnection(alternativeConnectionString ??
                                               Environment.GetEnvironmentVariable("FULLSTACK_PG_CONN")))
        {
            conn.Execute(@"
/* 
 
 if exists drop schema chat
 */
drop schema if exists chat cascade;
create schema chat;

create table chat.enduser
(
    id       integer generated always as identity
        constraint enduser_pk
            primary key,
    email    text,
    hash     text,
    salt     text,
    isbanned boolean default false,
    isadmin boolean default false
);
create table chat.messages
(
    id             integer generated always as identity
        constraint messages_pk
            primary key,
    messagecontent text,
    sender         integer default '-1':: integer not null
        constraint sender
            references chat.enduser,
    timestamp      timestamp with time zone,
    room           integer
);

create table chat.timeseries
(
    id        integer generated always as identity
        primary key,
    datapoint text,
    timestamp timestamp with time zone
);


INSERT INTO chat.enduser (email, hash, salt, isbanned, isadmin)
values ('bla@bla.dk', 'Uhq6WdmkqE+b3R84tTzFAprKxAOto3vhUx0HBG4J524=', 'G/Xx5vBlRMrF+oZcQ1vXiQ==', false, true);
");
        }
    }
}