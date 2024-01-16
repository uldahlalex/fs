using api.ClientEventHandlers;

namespace Tests;

public class StaticValues
{
    public const string DbRebuild = @"
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
    isbanned boolean default false
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


INSERT INTO chat.enduser (email, hash, salt, isbanned)
values ('bla@bla.dk', 'Uhq6WdmkqE+b3R84tTzFAprKxAOto3vhUx0HBG4J524=', 'G/Xx5vBlRMrF+oZcQ1vXiQ==', false);
";

    public static ClientWantsToAuthenticateDto AuthEvent = new()
    {
        email = "bla@bla.dk",
        password = "qweqweqwe"
    };

    public static ClientWantsToEnterRoomDto EnterRoomEvent = new()
    {
        roomId = 1
    };

    public static ClientWantsToSendMessageToRoomDto SendMessageEvent = new()
    {
        roomId = 1,
        messageContent = "hey"
    };
}