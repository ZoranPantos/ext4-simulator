namespace Ext4FileSystemSimulation;

class MemoryTracker
{
    private readonly int memoryDataLocation = 0;
    private readonly int diskCapacity = 20000000;
    private readonly int filesDataCapacity = 15500000;
    private readonly int fileNodesCapacity = 3000000;
    private readonly int dirNodesCapacity = 1000000;
    private readonly int memoryDataCapacity = 500000;
    private readonly int fileNodesStartAddress = 500000;
    private readonly int dirNodesStartAddress = 3500000;
    private readonly int fileDataStartAddress = 4500000;
    private readonly int maxFileSize = 64000;
    private readonly int minBlockSize = 5;
    private readonly int iDirNodeSize = 144;
    private readonly int iFileNodeSize = 152;

    public int IFileNodeSize { get { return iFileNodeSize; } }
    public int IDirNodeSize { get { return iDirNodeSize; } }
    public int MaxFileSize { get { return maxFileSize; } }
    public int MinBlockSize { get { return minBlockSize; } }
    public int FileDataStartAddress { get { return fileDataStartAddress; } }
    public int DirNodesStartAddress { get { return dirNodesStartAddress; } }
    public int FileNodesStartAddress { get { return fileNodesStartAddress; } }
    public int MemoryDataCapacity { get { return memoryDataCapacity; } }
    public int MemoryDataLocation { get { return memoryDataLocation; } }
    public int DiskCapacity { get { return diskCapacity; } }
    public int FileDataCapacity { get { return filesDataCapacity; } }
    public int FileNodesCapacity { get { return fileNodesCapacity; } }
    public int DirNodesCapacity { get { return dirNodesCapacity; } }
    public int OccupiedDiskSpace { get { return memoryDataCapacity + OccupiedDirNodeSpace + OccupiedFileNodeSpace + OccupiedFilesDataSpace; } }
    public int AvailableDiskSpace { get { return AvailableDirNodeSpace + AvailableFileNodeSpace + AvailableFilesDataSpace; } }
    public int OccupiedFilesDataSpace { get; set; }
    public int AvailableFilesDataSpace { get { return filesDataCapacity - OccupiedFilesDataSpace; } }
    public int OccupiedFileNodeSpace { get; set; }
    public int AvailableFileNodeSpace { get { return FileNodesCapacity - OccupiedFileNodeSpace; } }
    public int OccupiedDirNodeSpace { get; set; }
    public int AvailableDirNodeSpace { get { return DirNodesCapacity - OccupiedDirNodeSpace; } }
    public int NextAvailableFileNodeByte { get; set; }
    public int NextAvailableDirNodeByte { get; set; }
    public int FirstAvailableFileDataByte { get; set; }
    public int IFileNodeCount { get; set; }
    public int IDirNodeCount { get; set; }
}