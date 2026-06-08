namespace Ext4FileSystemSimulation.Enums;

internal enum FileNodeOffset : byte
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