using Dapper;
using Npgsql;

namespace Infrastructure;

public class ChatRepository(NpgsqlDataSource dataSource)
{
    public IEnumerable<Message> GetPastMessages()
    {
        var sql = $@"select * from chat.messages;";

        using (var conn = dataSource.OpenConnection())
        {
            return conn.Query<Message>(sql);
        }
        
    }
}

public class Message {

    public int Id { get; set; }
    public string MessageContent { get; set; }
}