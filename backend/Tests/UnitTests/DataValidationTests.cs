using System.ComponentModel.DataAnnotations;
using api.ClientEventHandlers;
using FluentAssertions;
using NUnit.Framework;

namespace Tests.UnitTests;

[TestFixture]
public class DataValidationTests
{
    [TestCase]
    public void Test1()
    {
        var dto = new ClientWantsToEnterRoomDto
        {
            roomId = -1
        };
        var validation = () => Validator.ValidateObject(dto, new ValidationContext(dto), true);
        validation.Should().Throw<ValidationException>();
    }

    [TestCase]
    public async Task ToxicityFilterDisallowsHate()
    {
        var dto = new ClientWantsToSendMessageToRoomDto
        {
            messageContent = "I hate you",
            roomId = 1
        };
        Assert.Throws<ValidationException>(() => dto.ValidateAsync().GetAwaiter().GetResult());
    }

    [TestCase]
    public async Task ToxicityFilterAllows()
    {
        var dto = new ClientWantsToSendMessageToRoomDto
        {
            messageContent = "I love you",
            roomId = 1
        };

        Assert.DoesNotThrowAsync(() => dto.ValidateAsync());
    }
}