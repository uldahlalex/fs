using api.Models.ServerEvents;
using api.StaticHelpers;
using lib;
using NUnit.Framework;
using Testcontainers.PostgreSql;

namespace Tests.IntegrationTests.ApiGranularTests;

[TestFixture]
public class RateLimiterTest
{
    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        await StaticHelpers.SetupTestClass(_postgreSqlContainer, false, true);
    }


    [OneTimeTearDown]
    public async Task DisposeAsync()
    {
        await _postgreSqlContainer.DisposeAsync();
    }

    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder().Build();

    [Test]
    public async Task AuthenticateRateLimiterRejectsTooMany()
    {
        //Should accept first 3 and trigger rate limiter on 4th
        var client = await new WebSocketTestClient().ConnectAsync();
        await client.DoAndAssert(StaticValues.AuthEvent, null);
        await client.DoAndAssert(StaticValues.AuthEvent, null);
        await client.DoAndAssert(StaticValues.AuthEvent, null);
        await client.DoAndAssert(StaticValues.AuthEvent, dtos =>
        {
            return dtos.Count(x => x.eventType.Equals(nameof(ServerSendsErrorMessageToClient))) == 1
                   && dtos.Count(x => x.eventType.Equals(nameof(ServerAuthenticatesUser))) == 3;
        });

        client.Client.Dispose();
    }
}