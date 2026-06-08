namespace Ext4FileSystemSimulation.Nodes;

internal sealed class DirectoryNode
{
    public int[] iFileArray;
    public int[] iSubDirArray;

    public int OrdinalNumber { get; set; }
    public int IDNumber { get; set; }
    public int FileCount { get; set; }
    public int DirCount { get; set; }
    public string Name { get; set; }
    public string Parent { get; set; }

    public DirectoryNode()
    {
        OrdinalNumber = IDNumber = FileCount = DirCount = 0;
        Name = Parent = null;
        iFileArray = new int[16];
        iSubDirArray = new int[8];
    }
}