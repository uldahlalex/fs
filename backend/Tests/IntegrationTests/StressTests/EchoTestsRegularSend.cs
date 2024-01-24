using System.Diagnostics;
using System.Text.Json;
using api.ClientEventHandlers;
using api.Models;
using api.Models.ServerEvents;
using FluentAssertions;
using NUnit.Framework;
using Testcontainers.PostgreSql;

namespace Tests.IntegrationTests.StressTests;

[TestFixture][Parallelizable(ParallelScope.None)]

public class EchoTestRegularSend
{
    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        await StaticHelpers.SetupTestClass(_postgreSqlContainer);
    }


    [OneTimeTearDown]
    public async Task DisposeAsync()
    {
        await _postgreSqlContainer.DisposeAsync();
    }

    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder().Build();

    [Test]
    public async Task ServerCanEchoManyTimes()
    {
        var numberOfEchos = 100_000;
        var history = new List<BaseDto>();
        var ws = await StaticHelpers.SetupWsClient(history);
        var stopwatch = Stopwatch.StartNew();
        Console.WriteLine("Time the stopwatch started: " + DateTime.Now);
        for (var i = 0; i < numberOfEchos; i++)
            ws.Send(JsonSerializer.Serialize(new ClientWantsToEchoDto
            {
                message = "test"
            }));

        while (history.Count() < numberOfEchos) Task.Delay(50).Wait();

        var expectedCount = history.Count(x => x.eventType == nameof(ServerEchosClient));
        expectedCount.Should().Be(numberOfEchos);
        stopwatch.Stop();
        Console.WriteLine("TIME FOR 1000 ECHOS: " + stopwatch.ElapsedMilliseconds + " milliseconds");
        Console.WriteLine("Time the stopwatch stopped: " + DateTime.Now);
    }
}