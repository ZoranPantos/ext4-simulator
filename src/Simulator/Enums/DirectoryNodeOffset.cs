namespace Ext4FileSystemSimulation.Enums;

internal enum DirectoryNodeOffset : byte
{
    OrdinalNum = 0,
    ID = 4,
    FileCount = 8,
    DirCount = 12,
    FileAddr = 16,
    DirAddr = 80,
    Name = 112,
    Parent = 128
}