using Dapper;
using Npgsql;
using NUnit.Framework;
using Testcontainers.PostgreSql;

namespace Tests.ApiTests;

[TestFixture]
public class MyDatabaseTests
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder().Build();

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        await _postgreSqlContainer.StartAsync();
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        Environment.SetEnvironmentVariable("FULLSTACK_TEST_PG_CONN", _postgreSqlContainer.GetConnectionString()); //todo

       // await ApiStartup.StartApi(null);

    }

    [OneTimeTearDown]
    public async Task DisposeAsync()
    {
        await _postgreSqlContainer.DisposeAsync();
    }



    [Test]
    public void Test()
    {
        using (var conn = new NpgsqlConnection(_postgreSqlContainer.GetConnectionString()))
        {
            var helloWorld = conn.ExecuteScalar<string>("select 'hello world'");
            Console.WriteLine(helloWorld);
        }
    }
    
}