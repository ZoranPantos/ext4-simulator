namespace Ext4FileSystemSimulation;

internal interface ISystemStorage
{
    void AddDirNode(string name);
    void AddFileNode(string path);
    void InsertDataToFile(string input, string path);
    void DisplayFileContent(string path);
    void PrintFileStats(string path);
    void ListPathContent(string path);
    void Rename(string path, string newName);
    void CopyFile(string fileName, string residentDir, string destinationDir);
    void MoveFile(string fileName, string residentDir, string destinationDir);
    void DeleteFileInDIR(string fileName, string dirName = null);
    void DeleteDirectory(string dirName);
    void DeleteAllFiles(string dirName);
    void FlushRootDirectory();
    void UploadFileToRoot(string fileName);
    void DownloadFileFromDirectory(string fileName, string dirName);
    void PrintDiskStats();
}