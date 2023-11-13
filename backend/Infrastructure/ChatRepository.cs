using core;
using Dapper;
using Npgsql;

namespace Infrastructure;

public class ChatRepository(NpgsqlDataSource dataSource)
{
    public IEnumerable<Message> GetPastMessages()
    {
        var sql = $@"
SELECT * FROM chat.messages ORDER BY timestamp DESC LIMIT 5;
";

        using (var conn = dataSource.OpenConnection())
        {
            return conn.Query<Message>(sql);
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
}
