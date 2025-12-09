using Scheduler.Domain.Entities.Calendars;

namespace Scheduler.Unit.Tests.DomainTests.Entities;

public class CalendarTests
{
    [Fact]
    public void CreateEntityWithValidColorShouldReturnCalendar()
    {
        // Arrange
        var validColor = "#A1B2C3";
        var name = "ValidName";

        // Act
        var result = Calendar.Create(name, validColor);

        // Assert
        result.Switch(
            entity =>
            {
                Assert.Equal(validColor, entity.Color);
                Assert.Equal(name, entity.Name);
            },
            ex => Assert.Fail($"Expected Calendar, but got exception: {ex.Message}"),
            ex => Assert.Fail($"Expected Calendar, but got exception: {ex.Message}")
        );
    }

    [Fact]
    public void CreateEntityWithInvalidColorShouldReturnFormatException()
    {
        // Arrange
        var invalidColor = "#ZZZZZZ";
        var name = "ValidName";

        // Act
        var result = Calendar.Create(name, invalidColor);

        // Assert
        result.Switch(
            entity => Assert.Fail("Expected FormatException, but got Calendar."),
            argEx => Assert.Fail($"Expected format exception"),
            fmtEx => Assert.IsType<FormatException>(fmtEx)
        );
    }

    [Fact]
    public void CreateEntityWithNullNameShouldReturnArgumentNullException()
    {
        // Arrange
        string nullName = null;

        // Act
        var result = Calendar.Create(nullName, "#A1B2C3");

        // Assert
        result.Switch(
            entity => Assert.Fail("Expected ArgumentException, but got Calendar."),
            argEx => Assert.IsType<ArgumentException>(argEx),
            fmtEx => Assert.Fail("Expected ArgumentException, but got Format.")
        );
    }

    [Fact]
    public void CreateEntityWithEmptyNameShouldReturnArgumentException()
    {
        // Arrange
        string emptyName = "";

        // Act
        var result = Calendar.Create(emptyName, "#A1B2C3");

        // Assert
        result.Switch(
            entity => Assert.Fail("Expected ArgumentException, but got Calendar."),
            argEx => Assert.IsType<ArgumentException>(argEx),
            fmtEx => Assert.Fail("Expected ArgumentException, but got Format.")
        );
    }
}
