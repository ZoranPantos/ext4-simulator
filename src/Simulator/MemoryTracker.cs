namespace Ext4FileSystemSimulation;

internal sealed class MemoryTracker
{
    public int FileNodeSize { get; } = 152;
    public int DirNodeSize { get; } = 144;
    public int MaxFileSize { get; } = 64000;
    public int MinBlockSize { get; } = 5;
    public int FileDataStartAddress { get; } = 4500000;
    public int DirNodesStartAddress { get; } = 3500000;
    public int FileNodesStartAddress { get; } = 500000;
    public int MemoryDataCapacity { get; } = 500000;
    public int MemoryDataLocation { get; } = 0;
    public int DiskCapacity { get; } = 20000000;
    public int FileDataCapacity { get; } = 15500000;
    public int FileNodesCapacity { get; } = 3000000;
    public int DirNodesCapacity { get; } = 1000000;
    public int OccupiedFilesDataSpace { get; }
    public int OccupiedFileNodeSpace { get; }
    public int OccupiedDirNodeSpace { get; set; }
    public int NextAvailableFileNodeByte { get; set; }
    public int NextAvailableDirNodeByte { get; set; }
    public int FirstAvailableFileDataByte { get; set; }
    public int FileNodeCount { get; set; }
    public int DirNodeCount { get; set; }
    public int OccupiedDiskSpace => MemoryDataCapacity + OccupiedDirNodeSpace + OccupiedFileNodeSpace + OccupiedFilesDataSpace;
    public int AvailableDiskSpace => AvailableDirNodeSpace + AvailableFileNodeSpace + AvailableFilesDataSpace;
    public int AvailableFilesDataSpace => FileDataCapacity - OccupiedFilesDataSpace;
    public int AvailableFileNodeSpace => FileNodesCapacity - OccupiedFileNodeSpace;
    public int AvailableDirNodeSpace => DirNodesCapacity - OccupiedDirNodeSpace;
}