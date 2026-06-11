using Ext4FileSystemSimulation.Strategies.CommandStrategies;
using Moq;

namespace Ext4FileSystemSimulation.UnitTests.Strategies;

public class NoArgumentCommandStrategyTests
{
    private readonly Mock<ITerminalContext> _context = new();
    private readonly Mock<ISystemStorage> _storage = new();
    private readonly NoArgumentCommandStrategy _sut;

    public NoArgumentCommandStrategyTests()
    {
        _context.Setup(c => c.Storage).Returns(_storage.Object);
        _sut = new NoArgumentCommandStrategy(_context.Object);
    }

    [Fact]
    public void Handle_UnrecognisedCommand_ReturnsFalse_AndDoesNotTouchStorage()
    {
        // Arrange
        _context.Setup(c => c.IsCommandValid("foo")).Returns(false);

        // Act
        bool result = _sut.Handle("foo");

        // Assert
        Assert.False(result);
        _storage.VerifyNoOtherCalls();
    }

    [Fact]
    public void Handle_Dstat_PrintsDiskStatsAndReturnsTrue()
    {
        // Arrange
        _context.Setup(c => c.IsCommandValid("dstat")).Returns(true);

        // Act
        bool result = _sut.Handle("dstat");

        // Assert
        Assert.True(result);
        _storage.Verify(s => s.PrintDiskStats(), Times.Once);
    }

    [Fact]
    public void Handle_Help_ReturnsTrue()
    {
        // Arrange
        _context.Setup(c => c.IsCommandValid("help")).Returns(true);

        // Act
        bool result = _sut.Handle("help");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Handle_RecognisedCommandThatRequiresArguments_ReturnsTrue()
    {
        // Arrange
        _context.Setup(c => c.IsCommandValid("mkdir")).Returns(true);

        // Act
        bool result = _sut.Handle("mkdir");

        // Assert
        Assert.True(result);
        _storage.VerifyNoOtherCalls();
    }
}