using api.Models.DbModels;
using api.Models.QueryModels;
using Dapper;
using Npgsql;

namespace api.Externalities;

public class ChatRepository(NpgsqlDataSource source)
{
    public async Task<IEnumerable<MessageWithSenderEmail>> GetPastMessages(int room, int lastMessageId = int.MaxValue) =>
    await (await source.OpenConnectionAsync()).QueryAsync<MessageWithSenderEmail>(@"
SELECT email, messagecontent, sender, messages.id as id, timestamp, room FROM chat.messages
join chat.enduser on chat.messages.sender = chat.enduser.id
where chat.messages.id<@lastMessageId and room=@room ORDER BY timestamp DESC LIMIT 5;", new { lastMessageId, room });
        
    
    /// pgSQL: INSERT INTO chat.messages (timestamp, sender, room, messageContent) values (now(), 1, 1, 'test') returning *;
    public Message InsertMessage(Message message) => source.OpenConnection().QueryFirst<Message>(@$"
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


    public async Task<EndUser> InsertUser(string email, string hash, string salt) =>
        await (await source.OpenConnectionAsync()).QueryFirstOrDefaultAsync<EndUser>(@"
insert into chat.enduser (email, hash, salt) values (@email, @hash, @salt) returning *;",
            new { email, hash, salt }) ?? throw new InvalidOperationException("Insertion and retrieval failed");

    public async Task<bool> DoesUserAlreadyExist(string email) =>
        await (await source.OpenConnectionAsync()).ExecuteScalarAsync<int>(@"
select count(*) from chat.enduser where email = @email;", new { email }) == 1;


    public async Task<EndUser> GetUser(string email) =>
        await (await source.OpenConnectionAsync()).QueryFirstOrDefaultAsync<EndUser>(@"
select * from chat.enduser where email = @email;", new { email }) ??
        throw new KeyNotFoundException("Could not find user with email " + email);
}