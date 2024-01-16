using api.Externalities;
using FluentAssertions;
using NUnit.Framework;

namespace Tests.UnitTests;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class AzureCognitiveServicesTests
{
    [TestCase]
    public Task Test1()
    {
        new AzureCognitiveServices().IsToxic("I hate you").Result.Any(a => a.severity > 0.8).Should().BeTrue();
        return Task.CompletedTask;
    }

    [TestCase]
    public Task Test2()
    {
        var enumerable = new AzureCognitiveServices().IsToxic("I love you").Result.Select(a => a.severity);
        enumerable.Any(a => a > 0.8).Should().BeFalse();
        return Task.CompletedTask;
    }
}