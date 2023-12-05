using core;
using core.Models;
using Dapper;
using Npgsql;

namespace Infrastructure;

public class ChatRepository(NpgsqlDataSource dataSource)
{
    public IEnumerable<Message> GetPastMessages(int room, int lastMessageId = int.MaxValue)
    {
        var sql = $@"
SELECT * FROM chat.messages where id<@lastMessageId and room=@room ORDER BY timestamp DESC LIMIT 5;
";

        using (var conn = dataSource.OpenConnection())
        {
            return conn.Query<Message>(sql, new {lastMessageId, room});
        }
    }

    public Message InsertMessage(Message message)
    {
        var sql = $@"
INSERT INTO chat.messages (timestamp, sender, room, messagecontent) 
values (@timestamp, @sender, @room, @messagecontent) 
returning *;";
        using (var conn = dataSource.OpenConnection())
        {
            return conn.QueryFirst<Message>(sql, message);
        }
    }

    public EndUser InsertUser(string email, string hash, string salt)
    {
        try
        {
            var sql = $@"insert into chat.enduser (email, hash, salt) values (@email, @hash, @salt) returning *;";
            using (var conn = dataSource.OpenConnection())
            {
                return conn.QueryFirst<EndUser>(sql, new { email, hash, salt });
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    public bool UserExists(string? email)
    {
        var sql = $@"select count(*) from chat.enduser where email = @email;";
        using (var conn = dataSource.OpenConnection())
        {
            return conn.ExecuteScalar<int>(sql, new { email }) == 1;
        }
    }

    public EndUser GetUser(string email)
    {
        var sql = $@"select * from chat.enduser where email = @email;";
        using (var conn = dataSource.OpenConnection())
        {
                 return conn.QueryFirst<EndUser>(sql, new { email });
                 
        }
    }

    public bool IsUserBanned(string email)
    {
        var sql = @"select isbanned from chat.enduser where email = @email;";
        using (var conn = dataSource.OpenConnection())
        {
            return conn.ExecuteScalar<bool>(sql, new { email = email });
        }
    }
}