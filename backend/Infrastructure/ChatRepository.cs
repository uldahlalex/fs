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

    public Message InsertMessage(Message message)
    {
        var sql = $@"insert into chat.messages (messagecontent) values (@messagecontent) returning *;";
        using (var conn = dataSource.OpenConnection())
        {
            return conn.QueryFirst<Message>(sql, new {messagecontent = message.MessageContent});
        }
    }
}

public class Message {

    public int Id { get; set; }
    public string MessageContent { get; set; }
}