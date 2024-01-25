using api;
using api.Models;
using api.Models.ServerEvents;
using NUnit.Framework;
using Testcontainers.PostgreSql;

namespace Tests.IntegrationTests.ApiGranularTests;

[TestFixture]
public class RateLimiterTest
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
    public async Task RateLimiterRejectsTooManyRequests()
    {
        var history = new List<BaseDto>();
        var ws = await StaticHelpers.SetupWsClient(history);

        await ws.DoAndWaitUntil(StaticValues.AuthEvent, new List<Func<bool>>
        {
            () => history.Count(x => x.eventType == nameof(ServerAuthenticatesUser)) == 1
        }, history);
        await ws.DoAndWaitUntil(StaticValues.AuthEvent, new List<Func<bool>>
        {
            () => history.Count(x => x.eventType == nameof(ServerAuthenticatesUser)) == 2
        }, history);
        await ws.DoAndWaitUntil(StaticValues.AuthEvent, new List<Func<bool>>
        {
            () => history.Count(x => x.eventType == nameof(ServerAuthenticatesUser)) == 3
        }, history);
        await ws.DoAndWaitUntil(StaticValues.AuthEvent, new List<Func<bool>>
        {
            () => history.Count(x => x.eventType == nameof(ServerSendsErrorMessageToClient)) == 1,
            () => history.Count(x => x.eventType == nameof(ServerAuthenticatesUser)) == 3
        }, history);
    }
}