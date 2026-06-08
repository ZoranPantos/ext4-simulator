namespace Ext4FileSystemSimulation.Enums;

internal enum DiskStatLocations : byte
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