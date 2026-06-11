using Ext4FileSystemSimulation.Strategies.CommandStrategies;
using Ext4FileSystemSimulation.Strategies.ValidationStrategies;
using Moq;

namespace Ext4FileSystemSimulation.UnitTests.Strategies;

public class TwoArgumentCommandStrategyTests
{
    private readonly Mock<ITerminalContext> _context = new();
    private readonly Mock<ISystemStorage> _storage = new();
    private readonly TwoArgumentCommandStrategy _sut;

    public TwoArgumentCommandStrategyTests()
    {
        _context.Setup(c => c.Storage).Returns(_storage.Object);

        var pathValidator = new PathValidationStrategy(_context.Object, "2");
        var twoPathValidator = new TwoPathValidationStrategy(_context.Object);

        _sut = new TwoArgumentCommandStrategy(_context.Object, pathValidator, twoPathValidator);
    }

    private void RecognisedCommand(string command) => _context.Setup(c => c.IsCommandValid(command)).Returns(true);
    private void AllPathsValid() => _context.Setup(c => c.InspectPath(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);
    private void AllPathsInvalid() => _context.Setup(c => c.InspectPath(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(false);

    [Fact]
    public void Handle_UnrecognisedCommand_ReturnsFalse_AndDoesNotTouchStorage()
    {
        // Arrange
        _context.Setup(c => c.IsCommandValid("foo")).Returns(false);

        // Act
        bool result = _sut.Handle("foo a/b c/d");

        // Assert
        Assert.False(result);
        _storage.VerifyNoOtherCalls();
    }

    [Fact]
    public void Handle_NeitherArgumentIsPath_ReturnsFalse()
    {
        // Arrange
        RecognisedCommand("echo");

        // Act
        bool result = _sut.Handle("echo foo bar");

        // Assert
        Assert.False(result);
        _storage.VerifyNoOtherCalls();
    }

    [Fact]
    public void Handle_Echo_ValidFileAndText_WritesDataAndReturnsTrue()
    {
        // Arrange
        RecognisedCommand("echo");
        AllPathsValid();

        // Act
        bool result = _sut.Handle("echo ROOT/notes.txt hello");

        // Assert
        Assert.True(result);
        _storage.Verify(s => s.InsertDataToFile("hello", "ROOT/notes.txt"), Times.Once);
    }

    [Fact]
    public void Handle_Echo_InvalidPath_ReturnsFalse()
    {
        // Arrange
        RecognisedCommand("echo");
        AllPathsInvalid();

        // Act
        bool result = _sut.Handle("echo ROOT/notes.txt hello");

        // Assert
        Assert.False(result);
        _storage.Verify(s => s.InsertDataToFile(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Handle_Echo_TextContainingSlash_IsRejected()
    {
        // Arrange
        RecognisedCommand("echo");
        AllPathsValid();

        // Act
        bool result = _sut.Handle("echo ROOT/notes.txt a/b");

        // Assert
        Assert.False(result);
        _storage.Verify(s => s.InsertDataToFile(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Handle_Rename_ValidPath_RenamesAndReturnsTrue()
    {
        // Arrange
        RecognisedCommand("rename");
        AllPathsValid();

        // Act
        bool result = _sut.Handle("rename ROOT/notes.txt n.txt");

        // Assert
        Assert.True(result);
        _storage.Verify(s => s.Rename("ROOT/notes.txt", "n.txt"), Times.Once);
    }

    [Fact]
    public void Handle_Rename_InvalidPath_ReturnsFalse()
    {
        // Arrange
        RecognisedCommand("rename");
        AllPathsInvalid();

        // Act
        bool result = _sut.Handle("rename ROOT/notes.txt n.txt");

        // Assert
        Assert.False(result);
        _storage.Verify(s => s.Rename(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Handle_Cp_TwoValidPaths_CopiesAndReturnsTrue()
    {
        // Arrange
        RecognisedCommand("cp");
        AllPathsValid();

        // Act
        bool result = _sut.Handle("cp ROOT/notes.txt ROOT/docs");

        // Assert
        Assert.True(result);
        _storage.Verify(s => s.CopyFile("notes.txt", "ROOT", "docs"), Times.Once);
    }

    [Fact]
    public void Handle_Cp_FromSubdirectoryIntoRoot_CopiesAndReturnsTrue()
    {
        // Arrange
        RecognisedCommand("cp");
        AllPathsValid();

        // Act
        bool result = _sut.Handle("cp ROOT/docs/todo.txt ROOT/");

        // Assert
        Assert.True(result);
        _storage.Verify(s => s.CopyFile("todo.txt", "docs", "ROOT"), Times.Once);
    }

    [Fact]
    public void Handle_Cp_FirstPathInvalid_ReturnsFalse()
    {
        // Arrange
        RecognisedCommand("cp");
        AllPathsValid();
        _context.Setup(c => c.InspectPath("ROOT/notes.txt", "cp", "2")).Returns(false);

        // Act
        bool result = _sut.Handle("cp ROOT/notes.txt ROOT/docs");

        // Assert
        Assert.False(result);
        _storage.Verify(s => s.CopyFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Handle_Cp_SecondPathInvalid_ReturnsFalse()
    {
        // Arrange
        RecognisedCommand("cp");
        AllPathsValid();
        _context.Setup(c => c.InspectPath("ROOT/docs", "cp", "2")).Returns(false);

        // Act
        bool result = _sut.Handle("cp ROOT/notes.txt ROOT/docs");

        // Assert
        Assert.False(result);
        _storage.Verify(s => s.CopyFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Handle_Mv_TwoValidPaths_MovesAndReturnsTrue()
    {
        // Arrange
        RecognisedCommand("mv");
        AllPathsValid();

        // Act
        bool result = _sut.Handle("mv ROOT/notes.txt ROOT/docs");

        // Assert
        Assert.True(result);
        _storage.Verify(s => s.MoveFile("notes.txt", "ROOT", "docs"), Times.Once);
    }

    [Fact]
    public void Handle_Mv_PathInvalid_ReturnsFalse()
    {
        // Arrange
        RecognisedCommand("mv");
        AllPathsInvalid();

        // Act
        bool result = _sut.Handle("mv ROOT/notes.txt ROOT/docs");

        // Assert
        Assert.False(result);
        _storage.Verify(s => s.MoveFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Handle_NoArgumentCommandGivenTwoArguments_ReturnsFalse()
    {
        // Arrange
        RecognisedCommand("clear");

        // Act
        bool result = _sut.Handle("clear ROOT/a ROOT/b");

        // Assert
        Assert.False(result);
        _storage.VerifyNoOtherCalls();
    }

    [Fact]
    public void Handle_OneArgumentCommandGivenTwoArguments_ReturnsFalse()
    {
        // Arrange
        RecognisedCommand("mkdir");

        // Act
        bool result = _sut.Handle("mkdir ROOT/a ROOT/b");

        // Assert
        Assert.False(result);
        _storage.VerifyNoOtherCalls();
    }
}