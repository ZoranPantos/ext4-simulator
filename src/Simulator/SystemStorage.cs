using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace OperatingSystemsProject2019;

public class SystemStorage
{
    private FileStream disk;
    private BinaryReader reader;
    private BinaryWriter writer;
    private MemoryTracker tracker;

    public SystemStorage()
    {
        InstantiateObjects();
        disk.Position = (long)MemoryTracker.DiskStatLocations.FreeIFileByteAddr;
        tracker.NextAvailableFileNodeByte = reader.ReadInt32();
        tracker.NextAvailableDirNodeByte = reader.ReadInt32();
        tracker.FirstAvailableFileDataByte = reader.ReadInt32();
        disk.Position = (int)MemoryTracker.DiskStatLocations.IFileCount;
        tracker.IFileNodeCount = reader.ReadInt32();
        disk.Position = (int)MemoryTracker.DiskStatLocations.IDirCount;
        tracker.IDirNodeCount = reader.ReadInt32();
        disk.Position = 0;
        WriteBaseStats(reader.ReadInt32() + 1);

        if (reader.ReadInt32() == 1)
        {
            CreateRoot();
        }
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
            tracker.IFileNodeCount = 0;
            tracker.IDirNodeCount = 0;
        }

        writer.Write(startup + 1); //Write methods uses sent type's needed allocation and not necessary 1b; example it will write this int into 4B = 32b
        writer.Write(tracker.DiskCapacity);
        writer.Write(tracker.FileDataCapacity);
        writer.Write(tracker.FileNodesCapacity);
        writer.Write(tracker.DirNodesCapacity);
        writer.Write(tracker.MemoryDataCapacity);
        writer.Write(tracker.FileNodesStartAddress);
        writer.Write(tracker.IFileNodeCount);
        writer.Write(tracker.DirNodesStartAddress);
        writer.Write(tracker.IDirNodeCount);
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

        for (int i = 0; i < 2; i++, writer.Write(0))
        {
            ;
        }

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
        {
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
        };

        for (int i = 0; i < 20; i++)
        {
            if (i == 1 || i == 2 || i == 3 || i == 4 || i == 5 || i == 11 || i == 12 || i == 13 || i == 14 || i == 15 || i == 16)
            {
                Console.WriteLine(messages[i] + reader.ReadInt32() + " B");
            }
            else
            {
                Console.WriteLine(messages[i] + reader.ReadInt32());
            }
        }

        Console.WriteLine();
        disk.Position = 0;
    }

    private void UpdateNodeCount(char type, char sign)
    {
        //Updates total number of nodes in RAM and HDD
        //type: f for file, d for dir; sign: + for increment, - for decrement
        UpdatePosition(type);
        int count = reader.ReadInt32();
        UpdatePosition(type);

        if (sign == '+')
        {
            writer.Write(count + 1);

            if (type == 'd')
            {
                tracker.IDirNodeCount++;
            }
            else if (type == 'f')
            {
                tracker.IFileNodeCount++;
            }
        }
        else if (sign == '-' && count > 0)
        {
            writer.Write(count - 1);

            if (type == 'd')
            {
                tracker.IDirNodeCount--;
            }
            else if (type == 'f')
            {
                tracker.IFileNodeCount--;
            }
        }
        else
        {
            return;
        }

        writer.Flush();
        disk.Position = 0;
    }

    private void UpdatePosition(char type)
    {
        if (type == 'f')
        {
            disk.Position = (int)MemoryTracker.DiskStatLocations.IFileCount;
        }
        else if (type == 'd')
        {
            disk.Position = (int)MemoryTracker.DiskStatLocations.IDirCount;
        }
    }

    private int GetLastUsedID(char type)
    {
        //f: file i-node; d: dir i-node
        if (type == 'f')
        {
            disk.Position = (int)MemoryTracker.DiskStatLocations.LastUsedINodeFileID;
        }
        else if (type == 'd')
        {
            disk.Position = (int)MemoryTracker.DiskStatLocations.LastUsedINodeDirID;
        }
        else
        {
            return 0;
        }

        int id = reader.ReadInt32();
        disk.Position = 0;

        return id;
    }

    private void UpdateLastUsedID(char type, int newID)
    {
        //f: file i-node; d: dir i-node
        if (type == 'f')
        {
            disk.Position = (int)MemoryTracker.DiskStatLocations.LastUsedINodeFileID;
        }
        else if (type == 'd')
        {
            disk.Position = (int)MemoryTracker.DiskStatLocations.LastUsedINodeDirID;
        }
        else
        {
            return;
        }

        writer.Write(newID);
        writer.Flush();
        disk.Position = 0;
    }

    private void CreateRoot()
    {
        disk.Position = (int)MemoryTracker.DiskStatLocations.FreeIDirByteAddr;
        int position = reader.ReadInt32();
        disk.Position = position;
        writer.Write(1);
        writer.Write(100);
        writer.Write(0);
        writer.Write(0);
        writer.Flush();
        disk.Position = (int)MemoryTracker.DiskStatLocations.FreeIDirByteAddr;
        position = reader.ReadInt32() + (int)MemoryTracker.INodeDirOffset.Name;
        disk.Position = position;
        writer.Write("ROOT");
        writer.Flush();
        disk.Position = (int)MemoryTracker.DiskStatLocations.FreeIDirByteAddr;
        position = reader.ReadInt32() + (int)MemoryTracker.INodeDirOffset.Parent;
        disk.Position = position;
        writer.Write("null");
        writer.Flush();
        UpdateNodeCount('d', '+');
        tracker.IDirNodeCount = 1;
        UpdateLastUsedID('d', 1);
        disk.Position = (int)MemoryTracker.DiskStatLocations.FreeIDirByteAddr;
        int value = reader.ReadInt32();
        disk.Position = (int)MemoryTracker.DiskStatLocations.FreeIDirByteAddr;
        writer.Write(value + tracker.IDirNodeSize);
        tracker.NextAvailableDirNodeByte = value + tracker.IDirNodeSize;
        disk.Position = (int)MemoryTracker.DiskStatLocations.LastUsedINodeDirID;
        writer.Write(100);
        writer.Flush();
        tracker.OccupiedDirNodeSpace = tracker.IDirNodeSize;
        disk.Position = 0;
    }

    public IDirNode GetDirStats(int address = 0)
    {
        //reads ROOT stats if nothing is sent as an argument
        IDirNode node = new IDirNode();
        if (address == 0)
        {
            disk.Position = tracker.DirNodesStartAddress;
        }
        else
        {
            disk.Position = address;
        }

        node.OrdinalNumber = reader.ReadInt32();
        node.IDNumber = reader.ReadInt32();
        node.FileCount = reader.ReadInt32();
        node.DirCount = reader.ReadInt32();

        for (int i = 0; i < 16; i++)
        {
            node.iFileArray[i] = reader.ReadInt32();
        }

        for (int i = 0; i < 8; i++)
        {
            node.iSubDirArray[i] = reader.ReadInt32();
        }

        node.Name = reader.ReadString();
        disk.Position = address + (int)MemoryTracker.INodeDirOffset.Parent;
        node.Parent = reader.ReadString();
        disk.Position = 0;

        return node;
    }

    public void UpdateRootStats(char filecount, char dircount, int newIDirNodeAddress, int newIFileNodeAddress)
    {
        //filecount and dircount: + increment, - decrement, o neutral
        int startAddress = tracker.DirNodesStartAddress + (int)MemoryTracker.INodeDirOffset.FileCount, value, arrayPosition;
        disk.Position = startAddress;

        if (filecount != 'o')
        {
            value = reader.ReadInt32();
            disk.Position = startAddress;

            if (filecount == '+')
            {
                writer.Write(value + 1);
                UpdateNodeCount('f', filecount);
                UpdateNextFreeIFileAddress(filecount);
            }
            else if (filecount == '-')
            {
                writer.Write(value - 1);
                UpdateNodeCount('f', filecount);
                UpdateNextFreeIFileAddress(filecount);
            }
        }

        startAddress = tracker.DirNodesStartAddress + (int)MemoryTracker.INodeDirOffset.DirCount;
        disk.Position = startAddress;

        if (dircount != 'o')
        {
            value = reader.ReadInt32();
            disk.Position = startAddress;

            if (dircount == '+')
            {
                writer.Write(value + 1);
                UpdateNodeCount('d', dircount);
                UpdateNextFreeIDirAddress(dircount);
            }
            else if (dircount == '-')
            {
                writer.Write(value - 1);
                UpdateNodeCount('d', dircount);
                UpdateNextFreeIDirAddress(dircount);
            }
        }

        IDirNode tmp = GetDirStats();

        if (dircount == '+' && newIDirNodeAddress != 0) //adding dir node pointer
        {
            arrayPosition = tracker.DirNodesStartAddress + (int)MemoryTracker.INodeDirOffset.iDirAddr + ((tmp.DirCount - 1) * sizeof(int));
            disk.Position = arrayPosition;
            writer.Write(newIDirNodeAddress);
        }
        else if (dircount == '-') //removing dir node pointer
        {
            arrayPosition = tracker.DirNodesStartAddress + (int)MemoryTracker.INodeDirOffset.iDirAddr + (tmp.DirCount * sizeof(int));
            disk.Position = arrayPosition;
            writer.Write(0);
        }
        if (filecount == '+' && newIFileNodeAddress != 0) //adding file node pointer
        {
            arrayPosition = tracker.DirNodesStartAddress + (int)MemoryTracker.INodeDirOffset.iFileAddr + ((tmp.FileCount - 1) * sizeof(int));
            disk.Position = arrayPosition;
            writer.Write(newIFileNodeAddress);
        }
        else if (filecount == '-') //removing file node pointer
        {
            arrayPosition = tracker.DirNodesStartAddress + (int)MemoryTracker.INodeDirOffset.iFileAddr + (tmp.FileCount * sizeof(int));
            disk.Position = 0;
            writer.Write(0);
        }

        writer.Flush();
    }

    private void UpdateNextFreeIDirAddress(char sign)
    {
        //Updates RAM and HDD memory
        disk.Position = (int)MemoryTracker.DiskStatLocations.FreeIDirByteAddr;

        if (sign == '+')
        {
            tracker.NextAvailableDirNodeByte += tracker.IDirNodeSize;
            writer.Write(tracker.NextAvailableDirNodeByte);
        }
        else if (sign == '-')
        {
            tracker.NextAvailableDirNodeByte -= tracker.IDirNodeSize;
            writer.Write(tracker.NextAvailableDirNodeByte);
        }

        writer.Flush();
    }

    public void AddIDirNode(string name) //mkdir
    {
        disk.Position = tracker.NextAvailableDirNodeByte;
        writer.Write(tracker.IDirNodeCount + 1);
        int newID = GetLastUsedID('d') + 1;
        disk.Position = tracker.NextAvailableDirNodeByte + (int)MemoryTracker.INodeDirOffset.ID;
        writer.Write(newID);
        UpdateLastUsedID('d', GetLastUsedID('d') + 1);
        disk.Position = tracker.NextAvailableDirNodeByte + (int)MemoryTracker.INodeDirOffset.FileCount;
        writer.Write(0);
        writer.Write(0);
        int namePosition = tracker.NextAvailableDirNodeByte + (int)MemoryTracker.INodeDirOffset.Name;
        disk.Position = namePosition;
        writer.Write(name);
        namePosition = tracker.NextAvailableDirNodeByte + (int)MemoryTracker.INodeDirOffset.Parent;
        disk.Position = namePosition;
        writer.Write("ROOT");
        UpdateRootStats('o', '+', tracker.NextAvailableDirNodeByte, 0);
        writer.Flush();
    }

    private void ListRootSubDirectories()
    {
        IDirNode root = GetDirStats();
        disk.Position = 0;

        for (int i = 0; i < root.DirCount; i++)
        {
            disk.Position = root.iSubDirArray[i] + (int)MemoryTracker.INodeDirOffset.Name;
            Console.WriteLine(reader.ReadString());
        }

        disk.Position = 0;
    }

    private void ListRootFiles()
    {
        IDirNode root = GetDirStats();
        disk.Position = 0;

        for (int i = 0; i < root.FileCount; i++)
        {
            disk.Position = root.iFileArray[i] + (int)MemoryTracker.INodeFileOffset.Name;
            Console.WriteLine(reader.ReadString());
        }
    }

    public void ListRootContent()
    {
        ListRootSubDirectories();
        ListRootFiles();
    }

    public void UpdateNextFreeIFileAddress(char sign)
    {
        //Updates RAM and HDD memory
        disk.Position = (int)MemoryTracker.DiskStatLocations.FreeIFileByteAddr;

        if (sign == '+')
        {
            tracker.NextAvailableFileNodeByte += tracker.IFileNodeSize;
            writer.Write(tracker.NextAvailableFileNodeByte);
        }
        else if (sign == '-')
        {
            tracker.NextAvailableFileNodeByte -= tracker.IFileNodeSize;
            writer.Write(tracker.NextAvailableFileNodeByte);
        }
        writer.Flush();
    }

    public void AddFileNodeToRoot(string name)
    {
        disk.Position = tracker.NextAvailableFileNodeByte;
        writer.Write(name);
        disk.Position = tracker.NextAvailableFileNodeByte + (int)MemoryTracker.INodeFileOffset.Directory;
        writer.Write("ROOT");
        disk.Position = tracker.NextAvailableFileNodeByte + (int)MemoryTracker.INodeFileOffset.Owner;
        writer.Write(Environment.UserName);
        disk.Position = tracker.NextAvailableFileNodeByte + (int)MemoryTracker.INodeFileOffset.Time;
        writer.Write(DateTime.Now.ToShortDateString());
        writer.Flush();
        UpdateRootStats('+', 'o', 0, tracker.NextAvailableFileNodeByte);
    }

    public int LookForDirectory(string name)
    {
        if (name.Equals("ROOT"))
        {
            return tracker.DirNodesStartAddress;
        }
        else if (!name.Equals("ROOT"))
        {
            IDirNode root = GetDirStats();
            for (int i = 0; i < root.DirCount; i++)
            {
                int checkPoint = root.iSubDirArray[i] + (int)MemoryTracker.INodeDirOffset.Name;
                disk.Position = checkPoint;

                if (reader.ReadString().Equals(name))
                {
                    return root.iSubDirArray[i];
                }
            }
        }

        return -1;
    }

    public void UpdateDirStats(int dirAddress, char change, int newFileNodeAddress = 0)
    {
        //change: '+' one file is added, '-' one file is deleted
        disk.Position = dirAddress + (int)MemoryTracker.INodeDirOffset.FileCount;
        int fileCount = reader.ReadInt32();

        if (change == '+')
        {
            disk.Position = dirAddress + (int)MemoryTracker.INodeDirOffset.iFileAddr + (fileCount * sizeof(int));
            writer.Write(newFileNodeAddress);
            disk.Position = dirAddress + (int)MemoryTracker.INodeDirOffset.FileCount;
            writer.Write(fileCount + 1);
            UpdateNodeCount('f', change);
            UpdateNextFreeIFileAddress(change);
        }
        else if (change == '-')
        {
            disk.Position = dirAddress + (int)MemoryTracker.INodeDirOffset.iFileAddr + ((fileCount - 1) * sizeof(int));
            writer.Write(0);
            disk.Position = dirAddress + (int)MemoryTracker.INodeDirOffset.FileCount;
            writer.Write(fileCount - 1);
            UpdateNodeCount('f', change);
            UpdateNextFreeIFileAddress(change);
        }

        writer.Flush();
    }

    public void ListDirFiles(string dirName)
    {
        IDirNode node = GetDirStats(LookForDirectory(dirName));
        disk.Position = 0;

        for (int i = 0; i < node.FileCount; i++)
        {
            disk.Position = node.iFileArray[i] + (int)MemoryTracker.INodeFileOffset.Name;
            Console.WriteLine(reader.ReadString());
        }
    }

    public void ListPathContent(string path) //ls
    {
        if (path.Equals("ROOT"))
        {
            ListRootContent();
        }
        else
        {
            string[] keys = path.Split("/");

            if (LookForDirectory(keys[1]) != -1)
            {
                ListDirFiles(keys[1]);
            }
            else
            {
                Terminal.ErrorMessage("{0} directory was not found", keys[1]);
            }
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
        disk.Position = tracker.NextAvailableFileNodeByte + (int)MemoryTracker.INodeFileOffset.Directory;
        writer.Write(dirName);
        disk.Position = tracker.NextAvailableFileNodeByte + (int)MemoryTracker.INodeFileOffset.Owner;
        writer.Write(Environment.UserName);
        disk.Position = tracker.NextAvailableFileNodeByte + (int)MemoryTracker.INodeFileOffset.Time;
        writer.Write(DateTime.Now.ToShortDateString());
        writer.Flush();
        UpdateDirStats(subrootAddr, '+', tracker.NextAvailableFileNodeByte);
    }

    public void AddIFileNode(string path) //create
    {
        string[] keys = path.Split('/');

        if (keys.Length == 2)
        {
            AddFileNodeToRoot(keys[1]);
        }
        else if (keys.Length == 3)
        {
            AddFileNodeToSubRoot(keys[2], keys[1]);
        }
        else
        {
            Terminal.ErrorMessage("Max level of depth is 2");
        }
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
            else
            {
                ChangeFileName(fileAddress, newName);
            }
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
                {
                    flag = false;
                }
                else
                {
                    flag = true;
                    ChangeFileName(fileAddress, newName);
                }
            }
            if (flag == false)
            {
                Terminal.ErrorMessage("Renaming failed");
            }
        }
        else
        {
            Terminal.ErrorMessage("Invalid path");
        }
    }

    private void ChangeFileName(int fileAddress, string newName)
    {
        disk.Position = fileAddress + (int)MemoryTracker.INodeFileOffset.Name;
        writer.Write(newName);
        writer.Flush();
    }

    private void ChangeDirectoryName(int dirAddress, string newName)
    {
        disk.Position = dirAddress + (int)MemoryTracker.INodeDirOffset.Name;
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
        else if (dirAddress == -1 && flag != 0)
        {
            return dirAddress;
        }
        else
        {
            IDirNode node = GetDirStats(dirAddress);

            if (node.FileCount < 1 && flag == 0)
            {
                Terminal.ErrorMessage("{0} directory does not contain any files", dirName);
                return -1;
            }
            else if (node.FileCount < 1 && flag != 0)
            {
                return -1;
            }
            else
            {
                for (int i = 0; i < node.FileCount; i++)
                {
                    disk.Position = node.iFileArray[i] + (int)MemoryTracker.INodeFileOffset.Name;
                    if (reader.ReadString().Equals(fileName))
                    {
                        return node.iFileArray[i];
                    }
                }
            }
        }
        if (flag == 0)
        {
            Terminal.ErrorMessage("{0} directory does not contain {1} file", dirName, fileName);
        }

        return -1;
    }

    public int[,] BlockHunter(int requestedFileSize)
    {
        if (requestedFileSize <= tracker.MaxFileSize)
        {
            int[,] resultMatrix = new int[2, IFileNode.MaxBlockCount];
            int byteCount, blockAddress, matIndex = 0;
            disk.Position = tracker.FileDataStartAddress;

            while (requestedFileSize > 0 && disk.Position < disk.Length && matIndex <= IFileNode.MaxBlockCount)
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
                            {
                                byteCount++;
                            }
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
        else
        {
            Terminal.ErrorMessage("File size was above the limit ({0} B /64000 B)", requestedFileSize.ToString());
            return null;
        }
    }

    private bool ValidateMinBlockSize(int byteAddress)
    {
        bool result = true;
        disk.Position = byteAddress;

        for (int i = 0; i < tracker.MinBlockSize; i++)
        {
            if (reader.ReadByte() != 0)
            {
                result = false;
            }
        }

        return result;
    }
    private int FreeByteArrayCount(int byteAddress, int requestedFileSize)
    {
        int count = 0;
        disk.Position = byteAddress;

        while (reader.ReadByte() == 0 && count < requestedFileSize)
        {
            count++;
        }

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
            (words.Length == 3 && LookForDirectory(words[1]) == -1 ||
            (LookForDirectory(words[1]) != -1 && SearchFileInDirectory(words[1], words[2]) == -1)))
        {
            Terminal.ErrorMessage("Specified file or directory does not exist");
            return;
        }
        else
        {
            if (words.Length == 2)
            {
                fileNodeAddress = SearchFileInDirectory(words[0], words[1]);
            }
            else if (words.Length == 3)
            {
                fileNodeAddress = SearchFileInDirectory(words[1], words[2]);
            }
        }

        int[,] matrix = BlockHunter(input.Length);

        if (matrix[0, 0] == 0)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("0 memory blocks found");
            Console.ForegroundColor = ConsoleColor.White;
            return;
        }

        int blockCount = 0;

        for (int i = 0; i < IFileNode.MaxBlockCount && matrix[0, i] != 0; i++, blockCount++)
        {
            ;
        }

        int fileSize = 0;
        disk.Position = fileNodeAddress + (int)MemoryTracker.INodeFileOffset.BlockCount;
        writer.Write(blockCount);
        disk.Position = fileNodeAddress + (int)MemoryTracker.INodeFileOffset.BlockAddr;

        for (int i = 0; i < blockCount; i++)
        {
            writer.Write(matrix[0, i]);
        }

        disk.Position = fileNodeAddress + (int)MemoryTracker.INodeFileOffset.BlockSizeAddr;

        for (int i = 0; i < blockCount; i++)
        {
            writer.Write(matrix[1, i]);
            fileSize += matrix[1, i];
        }

        disk.Position = fileNodeAddress + (int)MemoryTracker.INodeFileOffset.Size;
        writer.Write(fileSize);
        int index = 0, j = 0;

        while (blockCount > 0)
        {
            disk.Position = matrix[0, index];

            for (int i = 0; i < matrix[1, index]; i++)
            {
                if (j < input.Length)
                {
                    writer.Write(input[j++]);
                }
                else
                {
                    writer.Write((byte)('*'));
                }
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
        {
            address = SearchFileInDirectory(words[0], words[1]);
        }
        else if (words.Length == 3)
        {
            if (LookForDirectory(words[1]) == -1)
            {
                Terminal.ErrorMessage("{0} directory was not found", words[1]);
                return;
            }
            else
            {
                address = SearchFileInDirectory(words[1], words[2]);
            }
        }
        if (address == -1)
        {
            return;
        }
        else
        {
            disk.Position = address;
            Console.WriteLine("File name: {0}", reader.ReadString());
            disk.Position = address + (int)MemoryTracker.INodeFileOffset.Directory;
            Console.WriteLine("Directory: {0}", reader.ReadString());
            disk.Position = address + (int)MemoryTracker.INodeFileOffset.Owner;
            Console.WriteLine("Owner: {0}", reader.ReadString());
            disk.Position = address + (int)MemoryTracker.INodeFileOffset.Time;
            Console.WriteLine("Creation time: {0}", reader.ReadString());
            disk.Position = address + (int)MemoryTracker.INodeFileOffset.BlockCount;
            int blockCount = reader.ReadInt32();
            Console.WriteLine("Block count: {0}", blockCount);
            Console.WriteLine("File size: {0}", reader.ReadInt32());
            Console.Write("Pointers to blocks: ");

            for (int i = 0; i < blockCount; i++)
            {
                Console.Write("{0}, ", reader.ReadInt32());
            }

            disk.Position = address + (int)MemoryTracker.INodeFileOffset.BlockSizeAddr;
            Console.Write("\nBlock sizes: ");

            for (int i = 0; i < blockCount; i++)
            {
                Console.Write("{0}, ", reader.ReadInt32());
            }
        }
    }

    public void WriteZeros(int address, int size)
    {
        disk.Position = address;

        while (size-- > 0)
        {
            writer.Write((byte)0);
        }

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
        {
            address = SearchFileInDirectory(words[0], words[1]);
        }
        else if (words.Length == 3)
        {
            if (LookForDirectory(words[1]) == -1)
            {
                Terminal.ErrorMessage("{0} directory was not found", words[1]);
                return;
            }
            else
            {
                address = SearchFileInDirectory(words[1], words[2]);
            }
        }
        if (address == -1)
        {
            return;
        }
        else
        {
            string result = "";
            disk.Position = address + (int)MemoryTracker.INodeFileOffset.BlockCount;
            int blockCount = reader.ReadInt32();
            int[,] blockMatrix = new int[2, blockCount];
            disk.Position = address + (int)MemoryTracker.INodeFileOffset.BlockAddr;

            for (int i = 0; i < blockCount; i++)
            {
                blockMatrix[0, i] = reader.ReadInt32();
            }

            disk.Position = address + (int)MemoryTracker.INodeFileOffset.BlockSizeAddr;

            for (int i = 0; i < blockCount; i++)
            {
                blockMatrix[1, i] = reader.ReadInt32();
            }

            for (int i = 0; i < blockCount; i++)
            {
                disk.Position = blockMatrix[0, i];
                for (int j = 0; j < blockMatrix[1, i]; j++)
                {
                    result += reader.ReadChar();
                }
            }

            Console.WriteLine(result);
        }
    }

    private void DeleteFileData(int fileNodeAddress)
    {
        disk.Position = fileNodeAddress + (int)MemoryTracker.INodeFileOffset.Size;
        int size = reader.ReadInt32();

        if (size > 0)
        {
            disk.Position = fileNodeAddress + (int)MemoryTracker.INodeFileOffset.BlockCount;
            int blockCount = reader.ReadInt32();
            int[,] blockMatrix = new int[2, blockCount];
            disk.Position = fileNodeAddress + (int)MemoryTracker.INodeFileOffset.BlockAddr;

            for (int i = 0; i < blockCount; i++)
            {
                blockMatrix[0, i] = reader.ReadInt32();
            }

            disk.Position = fileNodeAddress + (int)MemoryTracker.INodeFileOffset.BlockSizeAddr;

            for (int i = 0; i < blockCount; i++)
            {
                blockMatrix[1, i] = reader.ReadInt32();
            }

            for (int i = 0; i < blockCount; i++)
            {
                WriteZeros(blockMatrix[0, i], blockMatrix[1, i]);
            }
        }
    }

    private void DeleteFileNode(int fileNodeAddress)
    {
        WriteZeros(fileNodeAddress, tracker.IFileNodeSize);
    }

    private void FileNodeAddressToZeroInDIR(int fileNodeAddress, int dirAddress = 0) //by deault it works with ROOT
    {
        if (dirAddress == 0)
        {
            disk.Position = tracker.DirNodesStartAddress + (int)MemoryTracker.INodeDirOffset.iFileAddr;
        }
        else
        {
            disk.Position = dirAddress + (int)MemoryTracker.INodeDirOffset.iFileAddr;
        }

        int tmpValue = 0, count = 16;

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

    private void RewriteFileNodeAddressArrayInDIR(int dirAddress = 0) //by default it works with ROOT
    {
        Queue<int> addrQueue = new();

        if (dirAddress == 0)
        {
            disk.Position = tracker.DirNodesStartAddress + (int)MemoryTracker.INodeDirOffset.iFileAddr;
        }
        else
        {
            disk.Position = dirAddress + (int)MemoryTracker.INodeDirOffset.iFileAddr;
        }

        int address;

        for (int i = 0; i < 16; i++)
        {
            address = reader.ReadInt32();

            if (address != 0)
            {
                addrQueue.Enqueue(address);
            }
        }
        if (dirAddress == 0)
        {
            WriteZeros(tracker.DirNodesStartAddress + (int)MemoryTracker.INodeDirOffset.iFileAddr, 16 * 4);
            disk.Position = tracker.DirNodesStartAddress + (int)MemoryTracker.INodeDirOffset.iFileAddr;
        }
        else
        {
            WriteZeros(dirAddress + (int)MemoryTracker.INodeDirOffset.iFileAddr, 16 * 4);
            disk.Position = dirAddress + (int)MemoryTracker.INodeDirOffset.iFileAddr;
        }

        int count = addrQueue.Count;

        for (int i = 0; i < count; i++)
        {
            writer.Write(addrQueue.Dequeue());
        }

        writer.Flush();
    }

    private void DecrementFileNodeUpdateToDIR(int dirAddress = 0) //by default works with ROOT
    {
        //just to file node and ram, not to the disc stats
        int address;

        if (dirAddress == 0)
        {
            address = tracker.DirNodesStartAddress + (int)MemoryTracker.INodeDirOffset.FileCount;
        }
        else
        {
            address = dirAddress + (int)MemoryTracker.INodeDirOffset.FileCount;
        }

        disk.Position = address;
        int count = reader.ReadInt32();
        disk.Position = address;
        writer.Write(count - 1);
        writer.Flush();
        tracker.IFileNodeCount--;
    }

    public void DeleteFileInDIR(string fileName, string dirName = null) //by default works with ROOT
    {
        int dirAddress = 0, fileAddress;

        if (dirName != null)
        {
            dirAddress = LookForDirectory(dirName);
            fileAddress = SearchFileInDirectory(dirName, fileName);
        }
        else
        {
            fileAddress = SearchFileInDirectory("ROOT", fileName);
        }

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
        else
        {
            IDirNode node = GetDirStats(dirAddress);

            if (node.FileCount > 0)
            {
                Queue<string> fileNames = new();

                for (int i = 0; i < node.FileCount; i++)
                {
                    disk.Position = node.iFileArray[i] + (int)MemoryTracker.INodeFileOffset.Name;
                    fileNames.Enqueue(reader.ReadString());
                }
                while (fileNames.Count > 0)
                {
                    DeleteFileInDIR(fileNames.Dequeue(), dirName);
                }
            }

            //next one is part for deleting dir node
            WriteZeros(dirAddress, tracker.IDirNodeSize);
            //dir node address to zero in ROOT
            disk.Position = tracker.DirNodesStartAddress + (int)MemoryTracker.INodeDirOffset.iDirAddr;
            int tmpValue = 0, count = 8;

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

            //rewrite dir node address array in ROOT
            Queue<int> addrQueue = new Queue<int>();
            disk.Position = tracker.DirNodesStartAddress + (int)MemoryTracker.INodeDirOffset.iDirAddr;
            int address;

            for (int i = 0; i < 8; i++)
            {
                address = reader.ReadInt32();
                if (address != 0)
                {
                    addrQueue.Enqueue(address);
                }
            }

            WriteZeros(tracker.DirNodesStartAddress + (int)MemoryTracker.INodeDirOffset.iDirAddr, 8 * 4);
            disk.Position = tracker.DirNodesStartAddress + (int)MemoryTracker.INodeDirOffset.iDirAddr;
            int count2 = addrQueue.Count;

            for (int i = 0; i < count2; i++)
            {
                writer.Write(addrQueue.Dequeue());
            }

            writer.Flush();
            //decrement dir node update to ROOT
            //just to file node and ram, not to the disc stats
            int address2;
            address2 = tracker.DirNodesStartAddress + (int)MemoryTracker.INodeDirOffset.DirCount;
            disk.Position = address2;
            int count3 = reader.ReadInt32();
            disk.Position = address2;
            writer.Write(count3 - 1);
            writer.Flush();
            tracker.IFileNodeCount--;
        }
    }

    public void DeleteAllFiles(string dirName)
    {
        int dirAddress = LookForDirectory(dirName);

        if (dirAddress == -1)
        {
            Terminal.ErrorMessage("{0} directory does not exist", dirName);
            return;
        }
        else
        {
            IDirNode node = GetDirStats(dirAddress);

            if (node.FileCount == 0)
            {
                Terminal.ErrorMessage("{0} directory does not contain any files", dirName);
                return;
            }
            else
            {
                Queue<string> fileNames = new();

                for (int i = 0; i < node.FileCount; i++)
                {
                    disk.Position = node.iFileArray[i] + (int)MemoryTracker.INodeFileOffset.Name;
                    fileNames.Enqueue(reader.ReadString());
                }
                while (fileNames.Count > 0)
                {
                    DeleteFileInDIR(fileNames.Dequeue(), dirName);
                }
            }
        }
    }

    public void FlushRootDirectory()
    {
        IDirNode node = GetDirStats();

        if (node.FileCount == 0 && node.DirCount == 0)
        {
            Terminal.ErrorMessage("ROOT directory was already empty");
            return;
        }
        else
        {
            if (node.FileCount > 0)
            {
                DeleteAllFiles("ROOT");
            }

            if (node.DirCount > 0)
            {
                Queue<string> dirNames = new();

                for (int i = 0; i < node.DirCount; i++)
                {
                    disk.Position = node.iSubDirArray[i] + (int)MemoryTracker.INodeDirOffset.Name;
                    dirNames.Enqueue(reader.ReadString());
                }
                while (dirNames.Count > 0)
                {
                    DeleteDirectory(dirNames.Dequeue());
                }
            }

            //cosmetic purposes:
            if (node.FileCount == 8 || node.DirCount > 4)
            {
                ProgressBarCOSMETIC();
            }
        }
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
            {
                Console.ForegroundColor = ConsoleColor.Green;
            }
        }

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine();
    }

    public void UploadFileToRoot(string fileName) //put
    {
        FileStream upload = new FileStream(fileName, FileMode.Open, FileAccess.Read);
        BinaryReader reader2 = new BinaryReader(upload);
        byte[] byteArray = new byte[upload.Length];

        for (int i = 0; i < upload.Length; i++)
        {
            byteArray[i] = reader2.ReadByte();
        }

        string input = System.Text.Encoding.Default.GetString(byteArray);
        AddFileNodeToRoot(fileName);
        InsertDataToFile(input, "ROOT/" + fileName);
        reader2.Close();
        upload.Close();
    }

    public void DownloadFileFromDirectory(string fileName, string dirName) //get
    {
        int dirAddress, fileAddress;
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
        {
            AddFileNodeToRoot(fileName);
        }
        else
        {
            AddFileNodeToSubRoot(fileName, destinationDir);
        }

        if (destinationDir.Equals("ROOT"))
        {
            InsertDataToFile(System.Text.Encoding.Default.GetString(output), "ROOT/" + fileName);
        }
        else
        {
            InsertDataToFile(System.Text.Encoding.Default.GetString(output), "ROOT/" + destinationDir + "/" + fileName);
        }
    }
    private byte[] ExtractFileData(int fileAddress)
    {
        int blockCount = 0;
        disk.Position = fileAddress + (int)MemoryTracker.INodeFileOffset.Size;
        byte[] output = new byte[reader.ReadInt32()];
        int byteIndex = 0;
        disk.Position = fileAddress + (int)MemoryTracker.INodeFileOffset.BlockCount;

        if (output.Length > 0)
        {
            blockCount = reader.ReadInt32();
        }

        int[,] matrix = new int[2, blockCount];
        disk.Position = fileAddress + (int)MemoryTracker.INodeFileOffset.BlockAddr;

        for (int i = 0; i < blockCount; i++)
        {
            matrix[0, i] = reader.ReadInt32();
        }

        disk.Position = fileAddress + (int)MemoryTracker.INodeFileOffset.BlockSizeAddr;

        for (int i = 0; i < blockCount; i++)
        {
            matrix[1, i] = reader.ReadInt32();
        }

        for (int i = 0; i < blockCount; i++)
        {
            disk.Position = matrix[0, i];
            for (int j = 0; j < matrix[1, i]; j++)
            {
                output[byteIndex++] = reader.ReadByte();
            }
        }

        return output;
    }

    public void MoveFile(string fileName, string residentDir, string destinationDir)
    {
        CopyFile(fileName, residentDir, destinationDir);

        if (residentDir.Equals("ROOT"))
        {
            DeleteFileInDIR(fileName);
        }
        else
        {
            DeleteFileInDIR(fileName, residentDir);
        }
    }
}