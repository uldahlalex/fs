using api.Externalities;
using FluentAssertions;
using NUnit.Framework;

namespace Tests;

[TestFixture]
public class AzureCognitiveServicesTests
{
    [TestCase]
    public async Task Test1()
    {
        new AzureCognitiveServices().IsToxic("I hate you").Result.Should().BeTrue();
    }

    [TestCase]
    public async Task Test2()
    {
        new AzureCognitiveServices().IsToxic("I love you").Result.Should().BeFalse();
    }
}