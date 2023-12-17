using api.Attributes;
using FluentAssertions;
using NUnit.Framework;

namespace Tests;

public class UnitTests
{
    [Test]
    public async Task Test1()
    {
        var result = await new ToxicityFilter().IsToxic("I hate you");
        result.Should().BeTrue();
    }

    [Test]
    public async Task Test2()
    {
        var result = await new ToxicityFilter().IsToxic("I love you");
        result.Should().BeFalse();
    }
}