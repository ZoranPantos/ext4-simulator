using Ext4FileSystemSimulation.Enums;
using Ext4FileSystemSimulation.Nodes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Ext4FileSystemSimulation;

internal sealed class SystemStorage : ISystemStorage
{
    private FileStream disk;
    private BinaryReader reader;
    private BinaryWriter writer;
    private MemoryTracker tracker;

    public SystemStorage()
    {
        InstantiateObjects();

        disk.Position = (long)DiskStatLocations.FreeFileByteAddr;

        tracker.NextAvailableFileNodeByte = reader.ReadInt32();
        tracker.NextAvailableDirNodeByte = reader.ReadInt32();
        tracker.FirstAvailableFileDataByte = reader.ReadInt32();

        disk.Position = (int)DiskStatLocations.FileCount;
        tracker.FileNodeCount = reader.ReadInt32();
        disk.Position = (int)DiskStatLocations.DirCount;
        tracker.DirNodeCount = reader.ReadInt32();
        disk.Position = 0;

        WriteBaseStats(reader.ReadInt32() + 1);

        if (reader.ReadInt32() == 1)
            CreateRoot();
    }

    private void InstantiateObjects()
    {
        disk = new FileStream("FILE SYSTEM", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
        tracker = new MemoryTracker();

        disk.SetLength(tracker.DiskCapacity);

        reader = new BinaryReader(disk);
        writer = new BinaryWriter(disk);
    }

    private void WriteBaseStats(int startup)
    {
        disk.Position = 0;

        if (--startup == 0)
        {
            tracker.NextAvailableFileNodeByte = tracker.FileNodesStartAddress;
            tracker.NextAvailableDirNodeByte = tracker.DirNodesStartAddress;
            tracker.FirstAvailableFileDataByte = tracker.FileDataStartAddress;
            tracker.FileNodeCount = 0;
            tracker.DirNodeCount = 0;
        }

        writer.Write(startup + 1);
        writer.Write(tracker.DiskCapacity);
        writer.Write(tracker.FileDataCapacity);
        writer.Write(tracker.FileNodesCapacity);
        writer.Write(tracker.DirNodesCapacity);
        writer.Write(tracker.MemoryDataCapacity);
        writer.Write(tracker.FileNodesStartAddress);
        writer.Write(tracker.FileNodeCount);
        writer.Write(tracker.DirNodesStartAddress);
        writer.Write(tracker.DirNodeCount);
        writer.Write(tracker.FileDataStartAddress);
        writer.Write(tracker.OccupiedDiskSpace);
        writer.Write(tracker.AvailableDiskSpace);
        writer.Write(tracker.OccupiedFileNodeSpace);
        writer.Write(tracker.AvailableFileNodeSpace);
        writer.Write(tracker.OccupiedDirNodeSpace);
        writer.Write(tracker.AvailableDirNodeSpace);
        writer.Write(tracker.NextAvailableFileNodeByte);
        writer.Write(tracker.NextAvailableDirNodeByte);
        writer.Write(tracker.FirstAvailableFileDataByte);

        writer.Write(0);
        writer.Write(0);

        writer.Flush();
        disk.Position = 0;
    }

    public void PrintDiskStats()
    {
        disk.Position = 0;

        Console.Write("DISK STATS ");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("(INCOMPLETE FUNCTIONALITY)");
        Console.ForegroundColor = ConsoleColor.White;

        string[] messages =
        [
            "_system boot-up number: ",
            "_disk capacity: ",
            "_file data capacity: ",
            "_file i-nodes capacity: ",
            "_directory i-nodes capacity: ",
            "_disk stats capacity: ",
            "_file i-nodes start address: ",
            "_file i-nodes count: ",
            "_directory i-nodes start address: ",
            "_directory i-nodes count: ",
            "_file data start address: ",
            "_occupied disk space: ",
            "_available disk space: ",
            "_occupied file i-node space: ",
            "_available file i-node space: ",
            "_occupied directory i-node space: ",
            "_available directory i-node space: ",
            "_next available file i-node byte: ",
            "_next available directory i-node byte: ",
            "_first available file data byte: "
        ];

        for (int i = 0; i < 20; i++)
        {
            if (i is (>= 1 and <= 5) or (>= 11 and <= 16))
                Console.WriteLine(messages[i] + reader.ReadInt32() + " B");
            else
                Console.WriteLine(messages[i] + reader.ReadInt32());
        }

        Console.WriteLine();
        disk.Position = 0;
    }

    private void UpdateNodeCount(char type, char sign)
    {
        // Updates total number of nodes in RAM and HDD
        // type: f for file, d for dir; sign: + for increment, - for decrement

        UpdatePosition(type);
        int count = reader.ReadInt32();
        UpdatePosition(type);

        if (sign.Equals('+'))
        {
            writer.Write(count + 1);

            if (type.Equals('d'))
                tracker.DirNodeCount++;
            else if (type.Equals('f'))
                tracker.FileNodeCount++;
        }
        else if (sign.Equals('-') && count > 0)
        {
            writer.Write(count - 1);

            if (type.Equals('d'))
                tracker.DirNodeCount--;
            else if (type.Equals('f'))
                tracker.FileNodeCount--;
        }
        else
            return;

        writer.Flush();
        disk.Position = 0;
    }

    private void UpdatePosition(char type)
    {
        if (type.Equals('f'))
            disk.Position = (int)DiskStatLocations.FileCount;
        else if (type.Equals('d'))
            disk.Position = (int)DiskStatLocations.DirCount;
    }

    private int GetLastUsedID(char type)
    {
        // f: file i-node; d: dir i-node

        if (type.Equals('f'))
            disk.Position = (int)DiskStatLocations.LastUsedNodeFileID;
        else if (type.Equals('d'))
            disk.Position = (int)DiskStatLocations.LastUsedNodeDirID;
        else
            return 0;

        int id = reader.ReadInt32();
        disk.Position = 0;

        return id;
    }

    private void UpdateLastUsedID(char type, int newID)
    {
        //f: file i-node; d: dir i-node

        if (type.Equals('f'))
            disk.Position = (int)DiskStatLocations.LastUsedNodeFileID;
        else if (type.Equals('d'))
            disk.Position = (int)DiskStatLocations.LastUsedNodeDirID;
        else
            return;

        writer.Write(newID);
        writer.Flush();

        disk.Position = 0;
    }

    private void CreateRoot()
    {
        disk.Position = (int)DiskStatLocations.FreeDirByteAddr;
        int position = reader.ReadInt32();
        disk.Position = position;

        writer.Write(1);
        writer.Write(100);
        writer.Write(0);
        writer.Write(0);
        writer.Flush();

        disk.Position = (int)DiskStatLocations.FreeDirByteAddr;
        position = reader.ReadInt32() + (int)DirectoryNodeOffset.Name;
        disk.Position = position;

        writer.Write("ROOT");
        writer.Flush();

        disk.Position = (int)DiskStatLocations.FreeDirByteAddr;
        position = reader.ReadInt32() + (int)DirectoryNodeOffset.Parent;
        disk.Position = position;

        writer.Write("null");
        writer.Flush();

        UpdateNodeCount('d', '+');
        tracker.DirNodeCount = 1;
        UpdateLastUsedID('d', 1);

        disk.Position = (int)DiskStatLocations.FreeDirByteAddr;
        int value = reader.ReadInt32();
        disk.Position = (int)DiskStatLocations.FreeDirByteAddr;

        writer.Write(value + tracker.DirNodeSize);

        tracker.NextAvailableDirNodeByte = value + tracker.DirNodeSize;
        disk.Position = (int)DiskStatLocations.LastUsedNodeDirID;

        writer.Write(100);
        writer.Flush();

        tracker.OccupiedDirNodeSpace = tracker.DirNodeSize;
        disk.Position = 0;
    }

    public DirectoryNode GetDirStats(int address = 0)
    {
        //reads ROOT stats if nothing is sent as an argument

        var node = new DirectoryNode();

        if (address == 0)
            disk.Position = tracker.DirNodesStartAddress;
        else
            disk.Position = address;

        node.OrdinalNumber = reader.ReadInt32();
        node.IDNumber = reader.ReadInt32();
        node.FileCount = reader.ReadInt32();
        node.DirCount = reader.ReadInt32();

        for (int i = 0; i < 16; i++)
            node.fileArray[i] = reader.ReadInt32();

        for (int i = 0; i < 8; i++)
            node.subDirArray[i] = reader.ReadInt32();

        node.Name = reader.ReadString();
        disk.Position = address + (int)DirectoryNodeOffset.Parent;
        node.Parent = reader.ReadString();
        disk.Position = 0;

        return node;
    }

    public void UpdateRootStats(char filecount, char dircount, int newDirNodeAddress, int newFileNodeAddress)
    {
        // filecount and dircount: + increment, - decrement, o neutral

        int startAddress = tracker.DirNodesStartAddress + (int)DirectoryNodeOffset.FileCount;
        int value;
        int arrayPosition;

        disk.Position = startAddress;

        if (!filecount.Equals('o'))
        {
            value = reader.ReadInt32();
            disk.Position = startAddress;

            if (filecount.Equals('+'))
            {
                writer.Write(value + 1);

                UpdateNodeCount('f', filecount);
                UpdateNextFreeFileAddress(filecount);
            }
            else if (filecount.Equals('-'))
            {
                writer.Write(value - 1);

                UpdateNodeCount('f', filecount);
                UpdateNextFreeFileAddress(filecount);
            }
        }

        startAddress = tracker.DirNodesStartAddress + (int)DirectoryNodeOffset.DirCount;
        disk.Position = startAddress;

        if (!dircount.Equals('o'))
        {
            value = reader.ReadInt32();
            disk.Position = startAddress;

            if (dircount.Equals('+'))
            {
                writer.Write(value + 1);

                UpdateNodeCount('d', dircount);
                UpdateNextFreeDirAddress(dircount);
            }
            else if (dircount.Equals('-'))
            {
                writer.Write(value - 1);

                UpdateNodeCount('d', dircount);
                UpdateNextFreeDirAddress(dircount);
            }
        }

        var directoryNodeTmp = GetDirStats();

        if (dircount.Equals('+') && newDirNodeAddress != 0) //adding dir node pointer
        {
            arrayPosition = tracker.DirNodesStartAddress + (int)DirectoryNodeOffset.DirAddr + ((directoryNodeTmp.DirCount - 1) * sizeof(int));
            disk.Position = arrayPosition;

            writer.Write(newDirNodeAddress);
        }
        else if (dircount.Equals('-')) //removing dir node pointer
        {
            arrayPosition = tracker.DirNodesStartAddress + (int)DirectoryNodeOffset.DirAddr + (directoryNodeTmp.DirCount * sizeof(int));
            disk.Position = arrayPosition;

            writer.Write(0);
        }
        if (filecount.Equals('+') && newFileNodeAddress != 0) //adding file node pointer
        {
            arrayPosition = tracker.DirNodesStartAddress + (int)DirectoryNodeOffset.FileAddr + ((directoryNodeTmp.FileCount - 1) * sizeof(int));
            disk.Position = arrayPosition;

            writer.Write(newFileNodeAddress);
        }
        else if (filecount.Equals('-')) //removing file node pointer
        {
            arrayPosition = tracker.DirNodesStartAddress + (int)DirectoryNodeOffset.FileAddr + (directoryNodeTmp.FileCount * sizeof(int));
            disk.Position = 0;

            writer.Write(0);
        }

        writer.Flush();
    }

    private void UpdateNextFreeDirAddress(char sign)
    {
        // Updates RAM and HDD memory

        disk.Position = (int)DiskStatLocations.FreeDirByteAddr;

        if (sign.Equals('+'))
        {
            tracker.NextAvailableDirNodeByte += tracker.DirNodeSize;
            writer.Write(tracker.NextAvailableDirNodeByte);
        }
        else if (sign.Equals('-'))
        {
            tracker.NextAvailableDirNodeByte -= tracker.DirNodeSize;
            writer.Write(tracker.NextAvailableDirNodeByte);
        }

        writer.Flush();
    }

    public void AddDirNode(string name) //mkdir
    {
        disk.Position = tracker.NextAvailableDirNodeByte;
        writer.Write(tracker.DirNodeCount + 1);
        int newID = GetLastUsedID('d') + 1;
        disk.Position = tracker.NextAvailableDirNodeByte + (int)DirectoryNodeOffset.ID;
        writer.Write(newID);
        UpdateLastUsedID('d', GetLastUsedID('d') + 1);
        disk.Position = tracker.NextAvailableDirNodeByte + (int)DirectoryNodeOffset.FileCount;
        writer.Write(0);
        writer.Write(0);
        int namePosition = tracker.NextAvailableDirNodeByte + (int)DirectoryNodeOffset.Name;
        disk.Position = namePosition;
        writer.Write(name);
        namePosition = tracker.NextAvailableDirNodeByte + (int)DirectoryNodeOffset.Parent;
        disk.Position = namePosition;
        writer.Write("ROOT");

        UpdateRootStats('o', '+', tracker.NextAvailableDirNodeByte, 0);

        writer.Flush();
    }

    private void ListRootSubDirectories()
    {
        var root = GetDirStats();
        disk.Position = 0;

        for (int i = 0; i < root.DirCount; i++)
        {
            disk.Position = root.subDirArray[i] + (int)DirectoryNodeOffset.Name;
            Console.WriteLine(reader.ReadString());
        }

        disk.Position = 0;
    }

    private void ListRootFiles()
    {
        var root = GetDirStats();
        disk.Position = 0;

        for (int i = 0; i < root.FileCount; i++)
        {
            disk.Position = root.fileArray[i] + (int)FileNodeOffset.Name;
            Console.WriteLine(reader.ReadString());
        }
    }

    public void ListRootContent()
    {
        ListRootSubDirectories();
        ListRootFiles();
    }

    public void UpdateNextFreeFileAddress(char sign)
    {
        //Updates RAM and HDD memory
        disk.Position = (int)DiskStatLocations.FreeFileByteAddr;

        if (sign.Equals('+'))
        {
            tracker.NextAvailableFileNodeByte += tracker.FileNodeSize;
            writer.Write(tracker.NextAvailableFileNodeByte);
        }
        else if (sign.Equals('-'))
        {
            tracker.NextAvailableFileNodeByte -= tracker.FileNodeSize;
            writer.Write(tracker.NextAvailableFileNodeByte);
        }

        writer.Flush();
    }

    public void AddFileNodeToRoot(string name)
    {
        disk.Position = tracker.NextAvailableFileNodeByte;
        writer.Write(name);
        disk.Position = tracker.NextAvailableFileNodeByte + (int)FileNodeOffset.Directory;
        writer.Write("ROOT");
        disk.Position = tracker.NextAvailableFileNodeByte + (int)FileNodeOffset.Owner;
        writer.Write(Environment.UserName);
        disk.Position = tracker.NextAvailableFileNodeByte + (int)FileNodeOffset.Time;
        writer.Write(DateTime.Now.ToShortDateString());

        writer.Flush();

        UpdateRootStats('+', 'o', 0, tracker.NextAvailableFileNodeByte);
    }

    public int LookForDirectory(string name)
    {
        if (name.Equals("ROOT"))
            return tracker.DirNodesStartAddress;

        var directoryNode = GetDirStats();

        for (int i = 0; i < directoryNode.DirCount; i++)
        {
            int checkPoint = directoryNode.subDirArray[i] + (int)DirectoryNodeOffset.Name;
            disk.Position = checkPoint;

            if (reader.ReadString().Equals(name))
                return directoryNode.subDirArray[i];
        }

        return -1;
    }

    public void UpdateDirStats(int dirAddress, char change, int newFileNodeAddress = 0)
    {
        //change: '+' one file is added, '-' one file is deleted
        disk.Position = dirAddress + (int)DirectoryNodeOffset.FileCount;
        int fileCount = reader.ReadInt32();

        if (change.Equals('+'))
        {
            disk.Position = dirAddress + (int)DirectoryNodeOffset.FileAddr + (fileCount * sizeof(int));
            writer.Write(newFileNodeAddress);
            disk.Position = dirAddress + (int)DirectoryNodeOffset.FileCount;
            writer.Write(fileCount + 1);

            UpdateNodeCount('f', change);
            UpdateNextFreeFileAddress(change);
        }
        else if (change.Equals('-'))
        {
            disk.Position = dirAddress + (int)DirectoryNodeOffset.FileAddr + ((fileCount - 1) * sizeof(int));
            writer.Write(0);
            disk.Position = dirAddress + (int)DirectoryNodeOffset.FileCount;
            writer.Write(fileCount - 1);

            UpdateNodeCount('f', change);
            UpdateNextFreeFileAddress(change);
        }

        writer.Flush();
    }

    public void ListDirFiles(string dirName)
    {
        DirectoryNode node = GetDirStats(LookForDirectory(dirName));
        disk.Position = 0;

        for (int i = 0; i < node.FileCount; i++)
        {
            disk.Position = node.fileArray[i] + (int)FileNodeOffset.Name;
            Console.WriteLine(reader.ReadString());
        }
    }

    public void ListPathContent(string path) //ls
    {
        if (path.Equals("ROOT"))
            ListRootContent();
        else
        {
            string[] keys = path.Split("/");

            if (LookForDirectory(keys[1]) != -1)
                ListDirFiles(keys[1]);
            else
                Terminal.ErrorMessage("{0} directory was not found", keys[1]);
        }
    }

    public void AddFileNodeToSubRoot(string fileName, string dirName)
    {
        int subrootAddr = LookForDirectory(dirName);

        if (subrootAddr == -1)
        {
            Terminal.ErrorMessage("{0} directory was not found", dirName);
            return;
        }

        disk.Position = tracker.NextAvailableFileNodeByte;
        writer.Write(fileName);
        disk.Position = tracker.NextAvailableFileNodeByte + (int)FileNodeOffset.Directory;
        writer.Write(dirName);
        disk.Position = tracker.NextAvailableFileNodeByte + (int)FileNodeOffset.Owner;
        writer.Write(Environment.UserName);
        disk.Position = tracker.NextAvailableFileNodeByte + (int)FileNodeOffset.Time;
        writer.Write(DateTime.Now.ToShortDateString());
        writer.Flush();
        UpdateDirStats(subrootAddr, '+', tracker.NextAvailableFileNodeByte);
    }

    public void AddFileNode(string path) //create
    {
        string[] keys = path.Split('/');

        if (keys.Length == 2)
            AddFileNodeToRoot(keys[1]);
        else if (keys.Length == 3)
            AddFileNodeToSubRoot(keys[2], keys[1]);
        else
            Terminal.ErrorMessage("Max level of depth is 2");
    }

    public void Rename(string path, string newName) //rename
    {
        string[] words = path.Split("/");

        if (words.Length == 3)
        {
            int fileAddress = SearchFileInDirectory(words[1], words[2]);

            if (fileAddress == -1)
            {
                Terminal.ErrorMessage("Renaming failed");
                return;
            }

            ChangeFileName(fileAddress, newName);
        }
        else if (words.Length == 2)
        {
            bool flag;
            int dirAddress = LookForDirectory(words[1]);

            if (dirAddress != -1)
            {
                flag = true;
                ChangeDirectoryName(dirAddress, newName);
            }
            else
            {
                int fileAddress = SearchFileInDirectory("ROOT", words[1]);

                if (fileAddress == -1)
                    flag = false;
                else
                {
                    flag = true;
                    ChangeFileName(fileAddress, newName);
                }
            }
            if (flag == false)
                Terminal.ErrorMessage("Renaming failed");
        }
        else
            Terminal.ErrorMessage("Invalid path");
    }

    private void ChangeFileName(int fileAddress, string newName)
    {
        disk.Position = fileAddress + (int)FileNodeOffset.Name;

        writer.Write(newName);
        writer.Flush();
    }

    private void ChangeDirectoryName(int dirAddress, string newName)
    {
        disk.Position = dirAddress + (int)DirectoryNodeOffset.Name;

        writer.Write(newName);
        writer.Flush();
    }

    public int SearchFileInDirectory(string dirName, string fileName, int flag = 0)
    {
        int dirAddress = LookForDirectory(dirName);

        if (dirAddress == -1 && flag == 0)
        {
            Terminal.ErrorMessage("{0} directory was not found", dirName);
            return dirAddress;
        }

        if (dirAddress == -1 && flag != 0)
            return dirAddress;

        var directoryNode = GetDirStats(dirAddress);

        if (directoryNode.FileCount < 1 && flag == 0)
        {
            Terminal.ErrorMessage("{0} directory does not contain any files", dirName);
            return -1;
        }

        if (directoryNode.FileCount < 1 && flag != 0)
            return -1;

        for (int i = 0; i < directoryNode.FileCount; i++)
        {
            disk.Position = directoryNode.fileArray[i] + (int)FileNodeOffset.Name;

            if (reader.ReadString().Equals(fileName))
                return directoryNode.fileArray[i];
        }

        if (flag == 0)
            Terminal.ErrorMessage("{0} directory does not contain {1} file", dirName, fileName);

        return -1;
    }

    public int[,] BlockHunter(int requestedFileSize)
    {
        if (requestedFileSize > tracker.MaxFileSize)
        {
            Terminal.ErrorMessage("File size was above the limit ({0} B /64000 B)", requestedFileSize.ToString());
            return null;
        }

        int[,] resultMatrix = new int[2, FileNode.MaxBlockCount];
        int byteCount;
        int blockAddress;
        int matIndex = 0;

        disk.Position = tracker.FileDataStartAddress;

        while (requestedFileSize > 0 && disk.Position < disk.Length && matIndex <= FileNode.MaxBlockCount)
        {
            if (reader.ReadByte() == 0 && ValidateMinBlockSize((int)disk.Position - 1))
            {
                blockAddress = (int)disk.Position - tracker.MinBlockSize;
                byteCount = FreeByteArrayCount(blockAddress, requestedFileSize);

                if (byteCount == requestedFileSize)
                {
                    if (requestedFileSize < tracker.MinBlockSize)
                    {
                        while (byteCount % tracker.MinBlockSize != 0)
                            byteCount++;
                    }

                    requestedFileSize -= byteCount;

                    resultMatrix[0, matIndex] = blockAddress;
                    resultMatrix[1, matIndex++] = byteCount;
                }
                else if (byteCount < requestedFileSize - 1)
                {
                    requestedFileSize -= byteCount;

                    resultMatrix[0, matIndex] = blockAddress;
                    resultMatrix[1, matIndex++] = byteCount;

                    disk.Position += byteCount;
                }
            }
        }

        return resultMatrix;
    }

    private bool ValidateMinBlockSize(int byteAddress)
    {
        bool result = true;
        disk.Position = byteAddress;

        for (int i = 0; i < tracker.MinBlockSize; i++)
        {
            if (reader.ReadByte() != 0)
                result = false;
        }

        return result;
    }
    
    private int FreeByteArrayCount(int byteAddress, int requestedFileSize)
    {
        int count = 0;
        disk.Position = byteAddress;

        while (reader.ReadByte() == 0 && count < requestedFileSize)
            count++;

        return count;
    }

    public void InsertDataToFile(string input, string path) //echo
    {
        string[] words = path.Split("/");
        int fileNodeAddress = 0;

        if (words.Length != 2 && words.Length != 3)
        {
            Terminal.ErrorMessage("Invalid path");
            return;
        }
        else if (words.Length == 2 && SearchFileInDirectory(words[0], words[1]) == -1 ||
            words.Length == 3 && LookForDirectory(words[1]) == -1 ||
            (LookForDirectory(words[1]) != -1 && SearchFileInDirectory(words[1], words[2]) == -1))
        {
            Terminal.ErrorMessage("Specified file or directory does not exist");
            return;
        }

        if (words.Length == 2)
            fileNodeAddress = SearchFileInDirectory(words[0], words[1]);
        else if (words.Length == 3)
            fileNodeAddress = SearchFileInDirectory(words[1], words[2]);

        int[,] matrix = BlockHunter(input.Length);

        if (matrix[0, 0] == 0)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("0 memory blocks found");
            Console.ForegroundColor = ConsoleColor.White;

            return;
        }

        int blockCount = 0;

        while (blockCount < FileNode.MaxBlockCount && matrix[0, blockCount] != 0)
            blockCount++;

        int fileSize = 0;

        disk.Position = fileNodeAddress + (int)FileNodeOffset.BlockCount;

        writer.Write(blockCount);

        disk.Position = fileNodeAddress + (int)FileNodeOffset.BlockAddr;

        for (int i = 0; i < blockCount; i++)
            writer.Write(matrix[0, i]);

        disk.Position = fileNodeAddress + (int)FileNodeOffset.BlockSizeAddr;

        for (int i = 0; i < blockCount; i++)
        {
            writer.Write(matrix[1, i]);
            fileSize += matrix[1, i];
        }

        disk.Position = fileNodeAddress + (int)FileNodeOffset.Size;
        writer.Write(fileSize);

        int index = 0;
        int j = 0;

        while (blockCount > 0)
        {
            disk.Position = matrix[0, index];

            for (int i = 0; i < matrix[1, index]; i++)
            {
                if (j < input.Length)
                    writer.Write(input[j++]);
                else
                    writer.Write((byte)'*');
            }

            blockCount--;
            index++;
        }

        writer.Flush();
    }

    public void PrintFileStats(string path) //stat
    {
        string[] words = path.Split("/");
        int address = -1;

        if (words.Length != 2 && words.Length != 3)
        {
            Terminal.ErrorMessage("Invalid path");
            return;
        }
        else if (words.Length == 2)
            address = SearchFileInDirectory(words[0], words[1]);
        else if (words.Length == 3)
        {
            if (LookForDirectory(words[1]) == -1)
            {
                Terminal.ErrorMessage("{0} directory was not found", words[1]);
                return;
            }

            address = SearchFileInDirectory(words[1], words[2]);
        }

        if (address == -1) return;

        disk.Position = address;
        Console.WriteLine("File name: {0}", reader.ReadString());
        disk.Position = address + (int)FileNodeOffset.Directory;
        Console.WriteLine("Directory: {0}", reader.ReadString());
        disk.Position = address + (int)FileNodeOffset.Owner;
        Console.WriteLine("Owner: {0}", reader.ReadString());
        disk.Position = address + (int)FileNodeOffset.Time;
        Console.WriteLine("Creation time: {0}", reader.ReadString());
        disk.Position = address + (int)FileNodeOffset.BlockCount;
        int blockCount = reader.ReadInt32();
        Console.WriteLine("Block count: {0}", blockCount);
        Console.WriteLine("File size: {0}", reader.ReadInt32());
        Console.Write("Pointers to blocks: ");

        for (int i = 0; i < blockCount; i++)
            Console.Write("{0}, ", reader.ReadInt32());

        disk.Position = address + (int)FileNodeOffset.BlockSizeAddr;
        Console.Write("\nBlock sizes: ");

        for (int i = 0; i < blockCount; i++)
            Console.Write("{0}, ", reader.ReadInt32());
    }

    public void WriteZeros(int address, int size)
    {
        disk.Position = address;

        while (size-- > 0)
            writer.Write((byte)0);

        writer.Flush();
    }

    public void DisplayFileContent(string path) //cat
    {
        string[] words = path.Split("/");
        int address = -1;

        if (words.Length != 2 && words.Length != 3)
        {
            Terminal.ErrorMessage("Invalid path");
            return;
        }
        else if (words.Length == 2)
            address = SearchFileInDirectory(words[0], words[1]);
        else if (words.Length == 3)
        {
            if (LookForDirectory(words[1]) == -1)
            {
                Terminal.ErrorMessage("{0} directory was not found", words[1]);
                return;
            }

            address = SearchFileInDirectory(words[1], words[2]);
        }

        if (address == -1) return;

        string result = "";

        disk.Position = address + (int)FileNodeOffset.BlockCount;

        int blockCount = reader.ReadInt32();
        int[,] blockMatrix = new int[2, blockCount];

        disk.Position = address + (int)FileNodeOffset.BlockAddr;

        for (int i = 0; i < blockCount; i++)
            blockMatrix[0, i] = reader.ReadInt32();

        disk.Position = address + (int)FileNodeOffset.BlockSizeAddr;

        for (int i = 0; i < blockCount; i++)
            blockMatrix[1, i] = reader.ReadInt32();

        for (int i = 0; i < blockCount; i++)
        {
            disk.Position = blockMatrix[0, i];

            for (int j = 0; j < blockMatrix[1, i]; j++)
                result += reader.ReadChar();
        }

        Console.WriteLine(result);
    }

    private void DeleteFileData(int fileNodeAddress)
    {
        disk.Position = fileNodeAddress + (int)FileNodeOffset.Size;
        int size = reader.ReadInt32();

        if (size > 0)
        {
            disk.Position = fileNodeAddress + (int)FileNodeOffset.BlockCount;

            int blockCount = reader.ReadInt32();
            int[,] blockMatrix = new int[2, blockCount];

            disk.Position = fileNodeAddress + (int)FileNodeOffset.BlockAddr;

            for (int i = 0; i < blockCount; i++)
                blockMatrix[0, i] = reader.ReadInt32();

            disk.Position = fileNodeAddress + (int)FileNodeOffset.BlockSizeAddr;

            for (int i = 0; i < blockCount; i++)
                blockMatrix[1, i] = reader.ReadInt32();

            for (int i = 0; i < blockCount; i++)
                WriteZeros(blockMatrix[0, i], blockMatrix[1, i]);
        }
    }

    private void DeleteFileNode(int fileNodeAddress) => WriteZeros(fileNodeAddress, tracker.FileNodeSize);

    // By default works with ROOT
    private void FileNodeAddressToZeroInDIR(int fileNodeAddress, int dirAddress = 0)
    {
        if (dirAddress == 0)
            disk.Position = tracker.DirNodesStartAddress + (int)DirectoryNodeOffset.FileAddr;
        else
            disk.Position = dirAddress + (int)DirectoryNodeOffset.FileAddr;

        int tmpValue = 0;
        int count = 16;

        while (tmpValue != fileNodeAddress && count > 0)
        {
            tmpValue = reader.ReadInt32();

            if (tmpValue == fileNodeAddress)
            {
                disk.Position -= 4;

                writer.Write(0);
                writer.Flush();

                return;
            }

            count--;
        }
    }

    // By default works with ROOT
    private void RewriteFileNodeAddressArrayInDIR(int dirAddress = 0)
    {
        Queue<int> addrQueue = new();

        if (dirAddress == 0)
            disk.Position = tracker.DirNodesStartAddress + (int)DirectoryNodeOffset.FileAddr;
        else
            disk.Position = dirAddress + (int)DirectoryNodeOffset.FileAddr;

        int address;

        for (int i = 0; i < 16; i++)
        {
            address = reader.ReadInt32();

            if (address != 0)
                addrQueue.Enqueue(address);
        }
        if (dirAddress == 0)
        {
            WriteZeros(tracker.DirNodesStartAddress + (int)DirectoryNodeOffset.FileAddr, 16 * 4);
            disk.Position = tracker.DirNodesStartAddress + (int)DirectoryNodeOffset.FileAddr;
        }
        else
        {
            WriteZeros(dirAddress + (int)DirectoryNodeOffset.FileAddr, 16 * 4);
            disk.Position = dirAddress + (int)DirectoryNodeOffset.FileAddr;
        }

        int count = addrQueue.Count;

        for (int i = 0; i < count; i++)
            writer.Write(addrQueue.Dequeue());

        writer.Flush();
    }

    // By default works with ROOT
    private void DecrementFileNodeUpdateToDIR(int dirAddress = 0)
    {
        int address;

        if (dirAddress == 0)
            address = tracker.DirNodesStartAddress + (int)DirectoryNodeOffset.FileCount;
        else
            address = dirAddress + (int)DirectoryNodeOffset.FileCount;

        disk.Position = address;
        int count = reader.ReadInt32();

        writer.Write(count - 1);
        writer.Flush();

        tracker.FileNodeCount--;
    }

    // By default works with ROOT
    public void DeleteFileInDIR(string fileName, string dirName = null)
    {
        int dirAddress = 0, fileAddress;

        if (dirName != null)
        {
            dirAddress = LookForDirectory(dirName);
            fileAddress = SearchFileInDirectory(dirName, fileName);
        }
        else
            fileAddress = SearchFileInDirectory("ROOT", fileName);

        if (fileAddress != -1)
        {
            DeleteFileData(fileAddress);
            DeleteFileNode(fileAddress);
            FileNodeAddressToZeroInDIR(fileAddress, dirAddress);
            RewriteFileNodeAddressArrayInDIR(dirAddress);
            DecrementFileNodeUpdateToDIR(dirAddress);
        }
    }

    public void DeleteDirectory(string dirName)
    {
        if (dirName.Equals("ROOT"))
        {
            Terminal.ErrorMessage("This directory cannot be deleted");
            return;
        }

        int dirAddress = LookForDirectory(dirName);

        if (dirAddress == -1)
        {
            Terminal.ErrorMessage("{0} directory was not found", dirName);
            return;
        }

        var directoryNode = GetDirStats(dirAddress);

        if (directoryNode.FileCount > 0)
        {
            Queue<string> fileNames = new();

            for (int i = 0; i < directoryNode.FileCount; i++)
            {
                disk.Position = directoryNode.fileArray[i] + (int)FileNodeOffset.Name;
                fileNames.Enqueue(reader.ReadString());
            }
            while (fileNames.Count > 0)
                DeleteFileInDIR(fileNames.Dequeue(), dirName);
        }

        WriteZeros(dirAddress, tracker.DirNodeSize);

        disk.Position = tracker.DirNodesStartAddress + (int)DirectoryNodeOffset.DirAddr;

        int tmpValue = 0;
        int count = 8;

        while (tmpValue != dirAddress && count > 0)
        {
            tmpValue = reader.ReadInt32();

            if (tmpValue == dirAddress)
            {
                disk.Position -= 4;

                writer.Write(0);
                writer.Flush();

                return;
            }

            count--;
        }

        Queue<int> addrQueue = new();

        disk.Position = tracker.DirNodesStartAddress + (int)DirectoryNodeOffset.DirAddr;
        int address;

        for (int i = 0; i < 8; i++)
        {
            address = reader.ReadInt32();

            if (address != 0)
                addrQueue.Enqueue(address);
        }

        WriteZeros(tracker.DirNodesStartAddress + (int)DirectoryNodeOffset.DirAddr, 8 * 4);

        disk.Position = tracker.DirNodesStartAddress + (int)DirectoryNodeOffset.DirAddr;
        int count2 = addrQueue.Count;

        for (int i = 0; i < count2; i++)
            writer.Write(addrQueue.Dequeue());

        writer.Flush();

        int address2 = tracker.DirNodesStartAddress + (int)DirectoryNodeOffset.DirCount;
        disk.Position = address2;

        int count3 = reader.ReadInt32();
        disk.Position = address2;

        writer.Write(count3 - 1);
        writer.Flush();

        tracker.FileNodeCount--;
    }

    public void DeleteAllFiles(string dirName)
    {
        int dirAddress = LookForDirectory(dirName);

        if (dirAddress == -1)
        {
            Terminal.ErrorMessage("{0} directory does not exist", dirName);
            return;
        }

        var directoryNode = GetDirStats(dirAddress);

        if (directoryNode.FileCount == 0)
        {
            Terminal.ErrorMessage("{0} directory does not contain any files", dirName);
            return;
        }

        Queue<string> fileNames = new();

        for (int i = 0; i < directoryNode.FileCount; i++)
        {
            disk.Position = directoryNode.fileArray[i] + (int)FileNodeOffset.Name;
            fileNames.Enqueue(reader.ReadString());
        }
        while (fileNames.Count > 0)
            DeleteFileInDIR(fileNames.Dequeue(), dirName);
    }

    public void FlushRootDirectory()
    {
        var directoryNode = GetDirStats();

        if (directoryNode.FileCount == 0 && directoryNode.DirCount == 0)
        {
            Terminal.ErrorMessage("ROOT directory was already empty");
            return;
        }

        if (directoryNode.FileCount > 0)
            DeleteAllFiles("ROOT");

        if (directoryNode.DirCount > 0)
        {
            Queue<string> dirNames = new();

            for (int i = 0; i < directoryNode.DirCount; i++)
            {
                disk.Position = directoryNode.subDirArray[i] + (int)DirectoryNodeOffset.Name;
                dirNames.Enqueue(reader.ReadString());
            }
            while (dirNames.Count > 0)
                DeleteDirectory(dirNames.Dequeue());
        }

        // Cosmetic purposes:
        if (directoryNode.FileCount == 8 || directoryNode.DirCount > 4)
            ProgressBarCOSMETIC();
    }

    private void ProgressBarCOSMETIC()
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Blue;

        for (int i = 0; i <= 100; i++)
        {
            Console.Write($"\rProgress: {i}%   ");
            Thread.Sleep(15);

            if (i == 99)
                Console.ForegroundColor = ConsoleColor.Green;
        }

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine();
    }

    public void UploadFileToRoot(string fileName) //put
    {
        var upload = new FileStream(fileName, FileMode.Open, FileAccess.Read);
        var binaryReader = new BinaryReader(upload);

        byte[] byteArray = new byte[upload.Length];

        for (int i = 0; i < upload.Length; i++)
            byteArray[i] = binaryReader.ReadByte();

        string input = System.Text.Encoding.Default.GetString(byteArray);

        AddFileNodeToRoot(fileName);
        InsertDataToFile(input, "ROOT/" + fileName);

        binaryReader.Close();
        upload.Close();
    }

    public void DownloadFileFromDirectory(string fileName, string dirName) //get
    {
        int dirAddress;
        int fileAddress;

        dirAddress = LookForDirectory(dirName);
        fileAddress = SearchFileInDirectory(dirName, fileName);

        if (dirAddress == -1)
        {
            Terminal.ErrorMessage("{0} directory was not found", dirName);
            return;
        }
        else if (fileAddress == -1)
        {
            Terminal.ErrorMessage("{0} file was not found", fileName);
            return;
        }

        byte[] output = ExtractFileData(fileAddress);

        FileStream download = new(fileName, FileMode.CreateNew, FileAccess.Write);
        BinaryWriter writer2 = new(download);

        writer2.Write(output);
        writer2.Flush();
        writer2.Close();

        download.Close();
    }

    public void CopyFile(string fileName, string residentDir, string destinationDir) //cp
    {
        if (SearchFileInDirectory(destinationDir, fileName, 1) != -1)
        {
            Terminal.ErrorMessage("{0} directory contains file with the same name", destinationDir);
            return;
        }

        int reDirAddress = LookForDirectory(residentDir);
        int deDirAddress = LookForDirectory(destinationDir);
        int fileAddress = SearchFileInDirectory(residentDir, fileName);

        if (reDirAddress == -1)
        {
            Terminal.ErrorMessage("{0} directory was not found", residentDir);
            return;
        }
        else if (deDirAddress == -1)
        {
            Terminal.ErrorMessage("{0} directory was not found", destinationDir);
            return;
        }
        else if (fileAddress == -1)
        {
            Terminal.ErrorMessage("{0} file was not found", fileName);
            return;
        }

        byte[] output = ExtractFileData(fileAddress);

        if (destinationDir.Equals("ROOT"))
            AddFileNodeToRoot(fileName);
        else
            AddFileNodeToSubRoot(fileName, destinationDir);

        if (destinationDir.Equals("ROOT"))
            InsertDataToFile(System.Text.Encoding.Default.GetString(output), "ROOT/" + fileName);
        else
            InsertDataToFile(System.Text.Encoding.Default.GetString(output), "ROOT/" + destinationDir + "/" + fileName);
    }

    private byte[] ExtractFileData(int fileAddress)
    {
        int blockCount = 0;
        int byteIndex = 0;

        disk.Position = fileAddress + (int)FileNodeOffset.Size;

        byte[] output = new byte[reader.ReadInt32()];
        disk.Position = fileAddress + (int)FileNodeOffset.BlockCount;

        if (output.Length > 0)
            blockCount = reader.ReadInt32();

        int[,] matrix = new int[2, blockCount];

        disk.Position = fileAddress + (int)FileNodeOffset.BlockAddr;

        for (int i = 0; i < blockCount; i++)
            matrix[0, i] = reader.ReadInt32();

        disk.Position = fileAddress + (int)FileNodeOffset.BlockSizeAddr;

        for (int i = 0; i < blockCount; i++)
            matrix[1, i] = reader.ReadInt32();

        for (int i = 0; i < blockCount; i++)
        {
            disk.Position = matrix[0, i];

            for (int j = 0; j < matrix[1, i]; j++)
                output[byteIndex++] = reader.ReadByte();
        }

        return output;
    }

    public void MoveFile(string fileName, string residentDir, string destinationDir)
    {
        CopyFile(fileName, residentDir, destinationDir);

        if (residentDir.Equals("ROOT"))
            DeleteFileInDIR(fileName);
        else
            DeleteFileInDIR(fileName, residentDir);
    }
}