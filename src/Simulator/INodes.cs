namespace OperatingSystemsProject2019;

public class IFileNode
{
    private readonly FileMetadata _metadata;

    //first row: block addresses; second row: block sizes (respectively)
    public int[,] ptrMatrix;

    public string CurrentDir { get; set; }
    public int CurrentBlockCount { get; set; }
    public static int MaxBlockCount => 10;
    public int FileSize { get; set; }

    public IFileNode()
    {
        _metadata = new FileMetadata();
        _metadata.Name = _metadata.Owner = _metadata.CreationTime = CurrentDir = null;
        CurrentBlockCount = FileSize = 0;
        ptrMatrix = new int[MaxBlockCount, MaxBlockCount];
    }
}

public class IDirNode
{
    public int[] iFileArray;
    public int[] iSubDirArray;

    public int OrdinalNumber { get; set; }
    public int IDNumber { get; set; }
    public int FileCount { get; set; }
    public int DirCount { get; set; }
    public string Name { get; set; }
    public string Parent { get; set; }

    public IDirNode()
    {
        OrdinalNumber = IDNumber = FileCount = DirCount = 0;
        Name = Parent = null;
        iFileArray = new int[16];
        iSubDirArray = new int[8];
    }
}

public class FileMetadata
{
    public string Name { get; set; }
    public string Owner { get; set; }
    public string CreationTime { get; set; }
}