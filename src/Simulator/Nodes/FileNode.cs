namespace Ext4FileSystemSimulation.Nodes;

public class FileNode
{
    private readonly FileMetadata _metadata;

    //first row: block addresses; second row: block sizes (respectively)
    public int[,] ptrMatrix;

    public string CurrentDir { get; set; }
    public int CurrentBlockCount { get; set; }
    public static int MaxBlockCount => 10;
    public int FileSize { get; set; }

    public FileNode()
    {
        _metadata = new FileMetadata();
        _metadata.Name = _metadata.Owner = _metadata.CreationTime = CurrentDir = null;
        CurrentBlockCount = FileSize = 0;
        ptrMatrix = new int[MaxBlockCount, MaxBlockCount];
    }
}

public class FileMetadata
{
    public string Name { get; set; }
    public string Owner { get; set; }
    public string CreationTime { get; set; }
}