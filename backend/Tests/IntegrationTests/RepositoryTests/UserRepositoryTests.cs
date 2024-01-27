using api;
using api.StaticHelpers;
using Dapper;
using Externalities;
using Externalities.ParameterModels;
using Externalities.QueryModels;
using FluentAssertions;
using Npgsql;
using NUnit.Framework;
using Testcontainers.PostgreSql;
using Utilities = Externalities.Utilities;

namespace Tests.IntegrationTests.RepositoryTests;

[TestFixture]
public class UserRepositoryTests
{
    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        await _postgreSqlContainer.StartAsync();
        Utilities.ExecuteRebuildFromSqlScript(_postgreSqlContainer.GetConnectionString());
        _chatRepository = new ChatRepository(
            new NpgsqlDataSourceBuilder(
                _postgreSqlContainer.GetConnectionString()).Build());
    }


    [OneTimeTearDown]
    public async Task DisposeAsync()
    {
        await _postgreSqlContainer.DisposeAsync();
    }

    private ChatRepository _chatRepository;

    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder().Build();

    [Test]
    public void Test()
    {
        var iterations = 10_000;
        var users = new List<EndUser>();
        for (var i = 0; i < iterations; i++) 
            users.Add(_chatRepository.GetUser(new FindByEmailParams("bla@bla.dk")));
        users.Count.Should().Be(iterations);
    }

   
}