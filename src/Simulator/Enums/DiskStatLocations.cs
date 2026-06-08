namespace Ext4FileSystemSimulation.Enums;

internal enum DiskStatLocations : byte
{
    Startups = 0,
    DiskCap = 4,
    FileDataCap = 8,
    FileINodesCap = 12,
    DirINodesCap = 16,
    MemDataCap = 20,
    FileStart = 24,
    FileCount = 28,
    DirStart = 32,
    DirCount = 36,
    FileDataStart = 40,
    OccDisk = 44,
    AvailDisk = 48,
    OccIFile = 52,
    AvailIFile = 56,
    OccIDir = 60,
    AvailIDir = 64,
    FreeFileByteAddr = 68,
    FreeDirByteAddr = 72,
    FreeFileDataByteAddr = 76,
    LastUsedNodeDirID = 80,
    LastUsedNodeFileID = 84
}