using System.Text.Json;
using api.Models;
using api.Models.ServerEvents;
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
    public async Task ServerCanHandleManyRequestsFromSameConnection()
    {
        var numberOfMessages = 1000;
        var history = new List<BaseDto>();
        var ws = await StaticHelpers.SetupWsClient(history);

        for (var i = 0; i < numberOfMessages; i++) ws.Send(JsonSerializer.Serialize(StaticValues.AuthEvent));

        while (history.Count() < numberOfMessages) Task.Delay(100).Wait();


        var expectedCount = history.Count(x => x.eventType == nameof(ServerAuthenticatesUser));
        expectedCount.Should().Be(numberOfMessages);
    }
}