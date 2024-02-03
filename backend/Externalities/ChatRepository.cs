using Dapper;
using Externalities.ParameterModels;
using Externalities.QueryModels;
using Npgsql;

namespace Externalities;

public class ChatRepository
{
    private readonly NpgsqlDataSource source;

    public ChatRepository(NpgsqlDataSource source)
    {
        this.source = source;
    }

    // pgSQL: SELECT email, messagecontent, sender, messages.id as id, timestamp, room FROM chat.messages join chat.enduser on chat.messages.sender = chat.enduser.id where chat.messages.id<1 and room=1 ORDER BY timestamp DESC LIMIT 5;
    public IEnumerable<MessageWithSenderEmail> GetPastMessages(GetPastMessagesParams getPastMessagesParams)
    {
        using var conn = source.OpenConnection();
        return conn.Query<MessageWithSenderEmail>(@$"
SELECT 
    email as {nameof(MessageWithSenderEmail.email)}, 
    messagecontent as {nameof(MessageWithSenderEmail.messageContent)}, 
    sender as {nameof(MessageWithSenderEmail.sender)}, 
    messages.id as {nameof(MessageWithSenderEmail.id)}, 
    timestamp as {nameof(MessageWithSenderEmail.timestamp)}, 
    room as {nameof(MessageWithSenderEmail.room)} 
FROM chat.messages
join chat.enduser on chat.messages.sender = chat.enduser.id
where chat.messages.id<@{nameof(GetPastMessagesParams.lastMessageId)} and room=@{nameof(getPastMessagesParams.room)} 
ORDER BY timestamp DESC LIMIT 5;", getPastMessagesParams);
    }

    // pgSQL: INSERT INTO chat.messages (timestamp, sender, room, messageContent) values (now(), 1, 1, 'test') returning *;
    public Message InsertMessage(Message message)
    {
        using var conn = source.OpenConnection();
        return conn.QueryFirst<Message>(@$"
INSERT INTO chat.messages (timestamp, sender, room, messageContent) 
values (@{nameof(Message.timestamp)}, 
        @{nameof(Message.sender)}, 
        @{nameof(Message.room)},
        @{nameof(Message.messageContent)}) 
returning 
    timestamp as {nameof(Message.timestamp)}, 
    sender as {nameof(Message.sender)}, 
    room as {nameof(Message.room)}, 
    messageContent as {nameof(Message.messageContent)},
    id as {nameof(Message.id)};", message);
    }

    // pgSQL: insert into chat.enduser (email, hash, salt, isbanned) values ('bla@bla.dk', 'Uhq6WdmkqE+b3R84tTzFAprKxAOto3vhUx0HBG4J524=', 'G/Xx5vBlRMrF+oZcQ1vXiQ==', false) returning *;
    public EndUser InsertUser(InsertUserParams insertUserParams)
    {
        using var conn = source.OpenConnection();
        return conn.QueryFirstOrDefault<EndUser>(@$"
insert into chat.enduser (email, hash, salt, isbanned, isadmin) 
values (
        @{nameof(InsertUserParams.email)}, 
        @{nameof(InsertUserParams.hash)}, 
        @{nameof(InsertUserParams.salt)}, 
        false, false) 
returning 
    email as {nameof(EndUser.email)}, 
    isbanned as {nameof(EndUser.isbanned)}, 
    isadmin as {nameof(EndUser.isadmin)},
    id as {nameof(EndUser.id)};", insertUserParams)
               ?? throw new InvalidOperationException("Insertion and retrieval failed");
    }

    // pgSQL: select count(*) from chat.enduser where email = 'bla@bla.dk';
    public bool DoesUserAlreadyExist(FindByEmailParams findByEmailParams)
    {
        using var conn = source.OpenConnection();
        return conn.ExecuteScalar<int>(@$"
select count(*) from chat.enduser where email = @{nameof(findByEmailParams.email)};", findByEmailParams) == 1;
    }

    // pgSQL: select * from chat.enduser where email = 'bla';
    public EndUser GetUser(FindByEmailParams findByEmailParams)
    {
        using var conn = source.OpenConnection();
        return conn.QueryFirstOrDefault<EndUser>($@"
                        select 
                            email as {nameof(EndUser.email)}, 
                            isbanned as {nameof(EndUser.isbanned)}, 
                            id as {nameof(EndUser.id)},
                            hash as {nameof(EndUser.hash)},
                            salt as {nameof(EndUser.salt)},
                            isadmin as {nameof(EndUser.isadmin)}
                        from chat.enduser where email = @{nameof(FindByEmailParams.email)};", findByEmailParams) ??
               throw new KeyNotFoundException("Could not find user with email " + findByEmailParams.email);
    }

    public bool IsMessageOwner(IsMessageOwnerParams isMessageOwnerParams)
    {
        using var conn = source.OpenConnection();
        return conn.ExecuteScalar<int>(@"
SELECT userid FROM chat.enduser 
JOIN chat.messages m on enduser.id = m.sender 
WHERE m.id = @messageid;", isMessageOwnerParams) == isMessageOwnerParams.userId;
    }

    public void DeleteMessage(DeleteMessageParams deleteMessageParams)
    {
        using (var conn = source.OpenConnection())
        {
            conn.Execute("DELETE FROM chat.messages WHERE id = @messageid;", deleteMessageParams);
        }
    }
}