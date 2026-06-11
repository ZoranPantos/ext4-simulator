using Ext4FileSystemSimulation.Strategies.CommandStrategies;
using Ext4FileSystemSimulation.Strategies.ValidationStrategies;
using Moq;

namespace Ext4FileSystemSimulation.UnitTests.Strategies;

public class OneArgumentCommandStrategyTests
{
    private readonly Mock<ITerminalContext> _context = new();
    private readonly Mock<ISystemStorage> _storage = new();
    private readonly List<string> _createdDirectories = [];
    private readonly OneArgumentCommandStrategy _sut;

    public OneArgumentCommandStrategyTests()
    {
        _context.Setup(c => c.Storage).Returns(_storage.Object);
        _context.Setup(c => c.CreatedSubDirectories).Returns(_createdDirectories);

        var pathValidator = new PathValidationStrategy(_context.Object, "2");
        var directoryCreationValidator = new DirectoryCreationValidationStrategy(_context.Object);

        _sut = new OneArgumentCommandStrategy(_context.Object, pathValidator, directoryCreationValidator);
    }

    private void RecognisedCommand(string command) => _context.Setup(c => c.IsCommandValid(command)).Returns(true);
    private void PathIsValid() => _context.Setup(c => c.InspectPath(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);
    private void PathIsInvalid() => _context.Setup(c => c.InspectPath(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(false);

    [Fact]
    public void Handle_UnrecognisedCommand_ReturnsFalse_AndDoesNotTouchStorage()
    {
        // Arrange
        _context.Setup(c => c.IsCommandValid("foo")).Returns(false);

        // Act
        bool result = _sut.Handle("foo bar");

        // Assert
        Assert.False(result);
        _storage.VerifyNoOtherCalls();
    }

    [Fact]
    public void Handle_Create_ValidPath_CreatesFileAndReturnsTrue()
    {
        // Arrange
        RecognisedCommand("create");
        PathIsValid();

        // Act
        bool result = _sut.Handle("create ROOT/notes.txt");

        // Assert
        Assert.True(result);
        _storage.Verify(s => s.AddFileNode("ROOT/notes.txt"), Times.Once);
    }

    [Fact]
    public void Handle_Cat_ValidPath_DisplaysContentAndReturnsTrue()
    {
        // Arrange
        RecognisedCommand("cat");
        PathIsValid();

        // Act
        bool result = _sut.Handle("cat ROOT/notes.txt");

        // Assert
        Assert.True(result);
        _storage.Verify(s => s.DisplayFileContent("ROOT/notes.txt"), Times.Once);
    }

    [Fact]
    public void Handle_Stat_ValidPath_PrintsStatsAndReturnsTrue()
    {
        // Arrange
        RecognisedCommand("stat");
        PathIsValid();

        // Act
        bool result = _sut.Handle("stat ROOT/notes.txt");

        // Assert
        Assert.True(result);
        _storage.Verify(s => s.PrintFileStats("ROOT/notes.txt"), Times.Once);
    }

    [Fact]
    public void Handle_Ls_RootPath_ListsRootContent()
    {
        // Arrange
        RecognisedCommand("ls");
        PathIsValid();

        // Act
        bool result = _sut.Handle("ls ROOT/");

        // Assert
        Assert.True(result);
        _storage.Verify(s => s.ListPathContent("ROOT"), Times.Once);
    }

    [Fact]
    public void Handle_Ls_DirectoryPath_ListsDirectoryContent()
    {
        // Arrange
        RecognisedCommand("ls");
        PathIsValid();

        // Act
        bool result = _sut.Handle("ls ROOT/docs");

        // Assert
        Assert.True(result);
        _storage.Verify(s => s.ListPathContent("ROOT/docs"), Times.Once);
    }

    [Fact]
    public void Handle_Get_ValidPath_DownloadsAndReturnsTrue()
    {
        // Arrange
        RecognisedCommand("get");
        PathIsValid();

        // Act
        bool result = _sut.Handle("get ROOT/notes.txt");

        // Assert
        Assert.True(result);
        _storage.Verify(s => s.DownloadFileFromDirectory("notes.txt", "ROOT"), Times.Once);
    }

    [Theory]
    [InlineData("create")]
    [InlineData("cat")]
    [InlineData("stat")]
    [InlineData("ls")]
    [InlineData("get")]
    public void Handle_PathValidatedCommand_InvalidPath_ReturnsFalse_AndDoesNotTouchStorage(string command)
    {
        // Arrange
        RecognisedCommand(command);
        PathIsInvalid();

        // Act
        bool result = _sut.Handle($"{command} ROOT/whatever");

        // Assert
        Assert.False(result);
        _storage.VerifyNoOtherCalls();
    }

    [Fact]
    public void Handle_Mkdir_ValidNewDirectory_AddsDirectoryAndTracksItAndReturnsTrue()
    {
        // Arrange
        RecognisedCommand("mkdir");
        PathIsValid();

        // Act
        bool result = _sut.Handle("mkdir ROOT/docs");

        // Assert
        Assert.True(result);
        _storage.Verify(s => s.AddDirNode("docs"), Times.Once);
        Assert.Contains("docs", _createdDirectories);
    }

    [Fact]
    public void Handle_Mkdir_InvalidPath_ReturnsFalse()
    {
        // Arrange
        RecognisedCommand("mkdir");
        PathIsInvalid();

        // Act
        bool result = _sut.Handle("mkdir badpath");

        // Assert
        Assert.False(result);
        _storage.Verify(s => s.AddDirNode(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Handle_Mkdir_ReservedRootName_ReturnsFalse()
    {
        // Arrange
        RecognisedCommand("mkdir");
        PathIsValid();

        // Act
        bool result = _sut.Handle("mkdir ROOT/ROOT");

        // Assert
        Assert.False(result);
        _storage.Verify(s => s.AddDirNode(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Handle_Mkdir_NameLongerThan15Characters_ReturnsFalse()
    {
        // Arrange
        RecognisedCommand("mkdir");
        PathIsValid();

        // Act
        bool result = _sut.Handle("mkdir ROOT/abcdefghijklmnop");

        // Assert
        Assert.False(result);
        _storage.Verify(s => s.AddDirNode(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Handle_Mkdir_DirectoryLimitReached_ReturnsFalse()
    {
        // Arrange
        RecognisedCommand("mkdir");
        PathIsValid();

        for (int i = 0; i < 8; i++)
            _createdDirectories.Add($"dir{i}");

        // Act
        bool result = _sut.Handle("mkdir ROOT/another");

        // Assert
        Assert.False(result);
        _storage.Verify(s => s.AddDirNode(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Handle_Mkdir_DuplicateName_ReturnsFalse()
    {
        // Arrange
        RecognisedCommand("mkdir");
        PathIsValid();
        _createdDirectories.Add("docs");

        // Act
        bool result = _sut.Handle("mkdir ROOT/docs");

        // Assert
        Assert.False(result);
        _storage.Verify(s => s.AddDirNode(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Handle_Rm_ValidFileInRoot_DeletesFile()
    {
        // Arrange
        RecognisedCommand("rm");
        PathIsValid();

        // Act
        _sut.Handle("rm ROOT/notes.txt");

        // Assert
        _storage.Verify(s => s.DeleteFileInDIR("notes.txt", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void Handle_Rm_KnownDirectory_DeletesDirectoryAndUntracksIt()
    {
        // Arrange
        RecognisedCommand("rm");
        PathIsValid();
        _createdDirectories.Add("docs");

        // Act
        _sut.Handle("rm ROOT/docs");

        // Assert
        _storage.Verify(s => s.DeleteDirectory("docs"), Times.Once);
        Assert.DoesNotContain("docs", _createdDirectories);
    }

    [Fact]
    public void Handle_Rm_InvalidPath_DoesNotDelete()
    {
        // Arrange
        RecognisedCommand("rm");
        PathIsInvalid();

        // Act
        _sut.Handle("rm badpath");

        // Assert
        _storage.VerifyNoOtherCalls();
    }

    [Fact]
    public void Handle_RmR_Root_FlushesRootAndReturnsTrue()
    {
        // Arrange
        RecognisedCommand("rm-r");
        PathIsValid();
        _createdDirectories.Add("docs");

        // Act
        bool result = _sut.Handle("rm-r ROOT/");

        // Assert
        Assert.True(result);
        _storage.Verify(s => s.FlushRootDirectory(), Times.Once);
        Assert.Empty(_createdDirectories);
    }

    [Fact]
    public void Handle_RmR_InvalidPath_ReturnsFalse()
    {
        // Arrange
        RecognisedCommand("rm-r");
        PathIsInvalid();

        // Act
        bool result = _sut.Handle("rm-r badpath");

        // Assert
        Assert.False(result);
        _storage.VerifyNoOtherCalls();
    }

    [Fact]
    public void Handle_Put_UploadsFileAndReturnsTrue()
    {
        // Arrange
        RecognisedCommand("put");

        // Act
        bool result = _sut.Handle("put report.txt");

        // Assert
        Assert.True(result);
        _storage.Verify(s => s.UploadFileToRoot("report.txt"), Times.Once);
    }

    [Fact]
    public void Handle_Put_WhenStorageThrows_ReturnsFalse()
    {
        // Arrange
        RecognisedCommand("put");
        _storage.Setup(s => s.UploadFileToRoot(It.IsAny<string>())).Throws(new InvalidOperationException("boom"));

        // Act
        bool result = _sut.Handle("put report.txt");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Handle_NoArgumentCommandGivenAnArgument_ReturnsFalse()
    {
        // Arrange
        RecognisedCommand("clear");

        // Act
        bool result = _sut.Handle("clear ROOT/x");

        // Assert
        Assert.False(result);
        _storage.VerifyNoOtherCalls();
    }

    [Fact]
    public void Handle_TwoArgumentCommandGivenOneArgument_ReturnsFalse()
    {
        // Arrange
        RecognisedCommand("rename");

        // Act
        bool result = _sut.Handle("rename ROOT/x");

        // Assert
        Assert.False(result);
        _storage.VerifyNoOtherCalls();
    }
}