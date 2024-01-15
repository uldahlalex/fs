using System.Text.Json;
using api.ClientEventHandlers;
using api.Extensions;
using api.Models;
using Websocket.Client;

namespace Tests;

public static class StaticHelpers
{
    public const string Url = "ws://localhost:8181";

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

    // public static List<Func<bool>> AreTheseDtosPresent(this List<BaseDto> history,
    //     (Type dto, int expectedFrequency)[] conditions)
    //     => conditions.Select(condition
    //             => (Func<bool>)(()
    //                 => history.Count(x
    //                     => x.GetType() == condition.dto) == condition.expectedFrequency)).ToList();

    public static Task DoAndWaitUntil<T>(
        this (WebsocketClient ws, List<BaseDto> communication) pair, 
        T action,
        List<Func<bool>> conditions) where T : BaseDto
    {
        pair.communication.Add(action);
        pair.ws.Send(JsonSerializer.Serialize(action, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        }));
        WaitForCondition(conditions).Wait();
        return Task.CompletedTask;
    }


    private static Task WaitForCondition(List<Func<bool>> conditions)
    {
        var startTime = DateTime.UtcNow;
        while (conditions.Any(x => !x.Invoke()))
        {
            var elapsedTime = DateTime.UtcNow - startTime;
            if (elapsedTime > TimeSpan.FromSeconds(5))
            {
                throw new TimeoutException($"Timeout. Unmet conditions");
            }
            Task.Delay(100).Wait();
        }

        return Task.CompletedTask;
    }
    public static async Task<(WebsocketClient ws, List<BaseDto> communication)> SetupWsClient(this WebsocketClient ws)
    {
        var communication = new List<BaseDto>();
        ws.MessageReceived.Subscribe(msg => { communication.Add(msg.Text!.DeserializeAndValidate<BaseDto>()); });
        await ws.Start();
        return (ws, communication);
    }

    public static string DbRebuild = @"
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
}