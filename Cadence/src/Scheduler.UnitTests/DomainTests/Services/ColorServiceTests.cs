using Scheduler.Domain.Services;

namespace Scheduler.Unit.Tests.DomainTests.Services;

public class ColorServiceTests
{
    [Fact]
    public void ColorServiceShouldReturnValidHexColor()
    {
        // Arrange
        var color = ColorService.GenerateRandomHexColor();

        // Act
        bool isValid = ColorService.IsValidHexColor(color);

        // Assert
        Assert.True(isValid, $"Generated color '{color}' is not a valid hex color.");
    }

    [Theory]
    [InlineData("#FFFFFF", true)]
    [InlineData("#000000", true)]
    [InlineData("#A1B2C3", true)]
    [InlineData("#a1b2c3", true)]
    [InlineData("#123ABC", true)]
    [InlineData("FFFFFF", false)]   // Missing #
    [InlineData("#GGGGGG", false)]  // Invalid hex characters
    [InlineData("#12345", false)]   // Too short
    [InlineData("#1234567", false)] // Too long
    public void ColorServiceShouldValidateCorrectly(string color, bool expected)
    {
        // Act
        bool isValid = ColorService.IsValidHexColor(color);

        // Assert
        Assert.Equal(expected, isValid);
    }
}
