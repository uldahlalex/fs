using System.Diagnostics;
using System.Text.Json;
using api;
using api.Models;
using api.Models.ServerEvents;
using api.StaticHelpers;
using FluentAssertions;
using NUnit.Framework;
using Testcontainers.PostgreSql;

namespace Tests.IntegrationTests.StressTests;

[TestFixture]
public class ManySignIns
{
    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        await StaticHelpers.SetupTestClass(_postgreSqlContainer, true);
    }


    [OneTimeTearDown]
    public async Task DisposeAsync()
    {
        await _postgreSqlContainer.DisposeAsync();
    }

    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder().Build();

    [Test]
    public async Task ServerCanHandleManyRequestsFromSameConnection() //todo problems when above 5000
    {
        var ws = await new WebSocketTestClient().ConnectAsync();
        var numberOfMessages = 1000;
        var stopwatch = Stopwatch.StartNew();
        Console.WriteLine("Time the stopwatch started: " + DateTime.Now);
        for (var i = 0; i < numberOfMessages-1; i++) 
            await ws.DoAndAssert(StaticValues.AuthEvent);
        await ws.DoAndAssert(StaticValues.AuthEvent, receivedMessages =>
        {
            return receivedMessages.Count(x => x.eventType.Equals(nameof(ServerAuthenticatesUser))) == numberOfMessages;
        });
        Console.WriteLine("TIME FOR "+numberOfMessages+" ECHOS: " + stopwatch.ElapsedMilliseconds + " milliseconds");
        Console.WriteLine("Time the stopwatch stopped: " + DateTime.Now);
    }
}