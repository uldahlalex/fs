using System.ComponentModel.DataAnnotations;
using api.ClientEventHandlers;
using FluentAssertions;
using NUnit.Framework;

namespace Tests.UnitTests;

[TestFixture]
[NonParallelizable]
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
}