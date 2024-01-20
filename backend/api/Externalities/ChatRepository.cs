using api.Models.DbModels;
using api.Models.QueryModels;
using Dapper;
using Npgsql;

namespace api.Externalities;

public class ChatRepository(NpgsqlDataSource dataSource)
{
    public IEnumerable<MessageWithSenderEmail> GetPastMessages(int room, int lastMessageId = int.MaxValue)
    {
        var sql = @"
SELECT email, messagecontent, sender, messages.id as id, timestamp, room FROM chat.messages
    join chat.enduser on chat.messages.sender = chat.enduser.id
       where chat.messages.id<@lastMessageId and room=@room ORDER BY timestamp DESC LIMIT 5;
";

        using (var conn = dataSource.OpenConnection())
        {
            return conn.Query<MessageWithSenderEmail>(sql, new { lastMessageId, room });
        }
    }

    public Message InsertMessage(int roomId, int sender, string messageContent)
    {
        var sql = @"
INSERT INTO chat.messages (timestamp, sender, room, messageContent) 
values (@timestamp, @sender, @room, @messageContent) 
returning *;";
        using (var conn = dataSource.OpenConnection())
        {
            return conn.QueryFirst<Message>(sql, new Message
            {
                timestamp = DateTimeOffset.UtcNow,
                room = roomId,
                sender = sender,
                messageContent = messageContent
            });
        }
    }

    public EndUser InsertUser(string email, string hash, string salt)
    {
        try
        {
            var sql = @"insert into chat.enduser (email, hash, salt) values (@email, @hash, @salt) returning *;";
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
        var sql = @"select count(*) from chat.enduser where email = @email;";
        using (var conn = dataSource.OpenConnection())
        {
            return conn.ExecuteScalar<int>(sql, new { email }) == 1;
        }
    }

    public EndUser GetUser(string email)
    {
        var sql = @"select * from chat.enduser where email = @email;";
        using (var conn = dataSource.OpenConnection())
        {
            return conn.QueryFirstOrDefault<EndUser>(sql, new { email }) ??
                   throw new KeyNotFoundException("Could not find user with email " + email);
        }
    }
    
}