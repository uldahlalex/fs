using api.Models.DbModels;
using api.Models.QueryModels;
using Dapper;
using Npgsql;

namespace api.Externalities;

public class ChatRepository(NpgsqlDataSource source)
{
    // pgSQL: SELECT email, messagecontent, sender, messages.id as id, timestamp, room FROM chat.messages
    // join chat.enduser on chat.messages.sender = chat.enduser.id
    // where chat.messages.id<1 and room=1 ORDER BY timestamp DESC LIMIT 5;
    public async Task<IEnumerable<MessageWithSenderEmail>>
        GetPastMessages(GetPastMessagesParams getPastMessagesParams) =>
        await (await source.OpenConnectionAsync()).QueryAsync<MessageWithSenderEmail>(@$"
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


    // pgSQL: INSERT INTO chat.messages (timestamp, sender, room, messageContent) values (now(), 1, 1, 'test') returning *;
    public async Task<Message> InsertMessage(Message message) =>
        await (await source.OpenConnectionAsync()).QueryFirstAsync<Message>(@$"
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


    // pgSQL: insert into chat.enduser (email, hash, salt, isbanned) values ('bla@bla.dk', 'Uhq6WdmkqE+b3R84tTzFAprKxAOto3vhUx0HBG4J524=', 'G/Xx5vBlRMrF+oZcQ1vXiQ==', false) returning *;
    public async Task<EndUser> InsertUser(InsertUserParams insertUserParams) =>
        await (await source.OpenConnectionAsync()).QueryFirstOrDefaultAsync<EndUser>(@$"
insert into chat.enduser (email, hash, salt, isbanned) 
values (
        @{nameof(InsertUserParams.email)}, 
        @{nameof(InsertUserParams.hash)}, 
        @{nameof(InsertUserParams.salt)}, 
        false) 
returning 
    email as {nameof(EndUser.email)}, 
    isbanned as {nameof(EndUser.isbanned)}, 
    id as {nameof(EndUser.id)};", insertUserParams)
        ?? throw new InvalidOperationException("Insertion and retrieval failed");

    // pgSQL: select count(*) from chat.enduser where email = 'bla@bla.dk';
    public async Task<bool> DoesUserAlreadyExist(FindByEmailParams findByEmailParams) =>
        await (await source.OpenConnectionAsync()).ExecuteScalarAsync<int>(@$"
select count(*) from chat.enduser where email = @{nameof(findByEmailParams.email)};",
            findByEmailParams) == 1;


    // pgSQL: select * from chat.enduser where email = 'bla';
    public async Task<EndUser>
        GetUser(FindByEmailParams findByEmailParams) //todo fix and also stress-test infra methods
    { //todo 2 check no using sync for pool exhaustion
        await using var conn = await source.OpenConnectionAsync();
        return await conn.QueryFirstOrDefaultAsync<EndUser>($@"
                        select 
                            email as {nameof(EndUser.email)}, 
                            isbanned as {nameof(EndUser.isbanned)}, 
                            id as {nameof(EndUser.id)},
                            hash as {nameof(EndUser.hash)},
                            salt as {nameof(EndUser.salt)}
                        from chat.enduser where email = @{nameof(FindByEmailParams.email)};", findByEmailParams) ??
               throw new KeyNotFoundException("Could not find user with email " + findByEmailParams.email);
    }
}