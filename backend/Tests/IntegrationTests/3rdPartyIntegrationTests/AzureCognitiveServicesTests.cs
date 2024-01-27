using Commons;
using Externalities;
using Externalities._3rdPartyTransferModels;
using FluentAssertions;
using NUnit.Framework;

namespace Tests.IntegrationTests._3rdPartyIntegrationTests;

[TestFixture]
public class AzureCognitiveServicesTests
{
    [TestCase]
    public Task Test1()
    {
        new AzureCognitiveServices()
            .GetToxicityAnalysis("I hate you")
            .Result.Deserialize<ToxicityResponse>()
            .categoriesAnalysis
            .Any(a => a.severity > 0.8)
            .Should()
            .BeTrue();
        return Task.CompletedTask;
    }

    [TestCase]
    public Task Test2()
    {
        new AzureCognitiveServices()
            .GetToxicityAnalysis("I love you")
            .Result.Deserialize<ToxicityResponse>()
            .categoriesAnalysis
            .Any(a => a.severity > 0.8)
            .Should()
            .BeFalse();
        return Task.CompletedTask;
    }
}