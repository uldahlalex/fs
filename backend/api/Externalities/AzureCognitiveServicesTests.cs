#if !RELEASE
using FluentAssertions;
using NUnit.Framework;

namespace api.Externalities;

public class AzureCognitiveServicesTests
{

    [Test]
    public void Test()
    {
        new AzureCognitiveServices().IsToxic("I hate you").Result.Should().BeTrue();
    }

    [Test]
    public void Test2()
    {
        new AzureCognitiveServices().IsToxic("I love you").Result.Should().BeFalse();
    }

}
#endif