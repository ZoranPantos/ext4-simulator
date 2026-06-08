namespace OperatingSystemsProject2019;

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

    public enum DiskStatLocations : byte
    {
        Startups = 0,
        DiskCap = 4,
        FileDataCap = 8,
        FileINodesCap = 12,
        DirINodesCap = 16,
        MemDataCap = 20,
        IFileStart = 24,
        IFileCount = 28,
        IDirStart = 32,
        IDirCount = 36,
        FileDataStart = 40,
        OccDisk = 44,
        AvailDisk = 48,
        OccIFile = 52,
        AvailIFile = 56,
        OccIDir = 60,
        AvailIDir = 64,
        FreeIFileByteAddr = 68,
        FreeIDirByteAddr = 72,
        FreeFileDataByteAddr = 76,
        LastUsedINodeDirID = 80,
        LastUsedINodeFileID = 84
    }

    public enum INodeDirOffset : byte
    {
        OrdinalNum = 0,
        ID = 4,
        FileCount = 8,
        DirCount = 12,
        iFileAddr = 16,
        iDirAddr = 80,
        Name = 112,
        Parent = 128
    }

    public enum INodeFileOffset : byte
    {
        Name = 0,
        Directory = 16,
        Owner = 32,
        Time = 48,
        BlockCount = 64,
        Size = 68,
        BlockAddr = 72,
        BlockSizeAddr = 112
    }
}