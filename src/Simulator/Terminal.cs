using System;
using System.Collections.Generic;

namespace OperatingSystemsProject2019;

public class Terminal
{
    private readonly SystemStorage disk;
    private readonly LinkedList<string> availableCommands, createdSubDirectories;
    private string userInput;

    public Terminal(SystemStorage disk)
    {
        this.disk = disk;
        availableCommands = new LinkedList<string>();
        createdSubDirectories = new LinkedList<string>();
        availableCommands.AddLast("mkdir");
        availableCommands.AddLast("create");
        availableCommands.AddLast("put");
        availableCommands.AddLast("get");
        availableCommands.AddLast("ls");
        availableCommands.AddLast("cp");
        availableCommands.AddLast("mv");
        availableCommands.AddLast("rename");
        availableCommands.AddLast("echo");
        availableCommands.AddLast("cat");
        availableCommands.AddLast("rm");
        availableCommands.AddLast("rm-r");
        availableCommands.AddLast("stat");
        availableCommands.AddLast("dstat");
        availableCommands.AddLast("help");
        availableCommands.AddLast("clear");
        availableCommands.AddLast("exit");
    }

    private void Help()
    {
        Console.WriteLine("\nAVAILABLE COMMANDS:");
        Console.WriteLine("mkdir [path]: creates new directory on specified path");
        Console.WriteLine("create [path]: creates new file on specified path");
        Console.WriteLine("put [path/.../file_name]: uploads file from host file system to guest file system");
        Console.WriteLine("get [path/.../file_name]: downloads file on specified path from guest file system to host file system");
        Console.WriteLine("ls [path]: prints directory content on specified path");
        Console.WriteLine("cp [path/.../file_name] [path/.../dir_name]: creates copy of file on specified path");
        Console.WriteLine("mv [path/.../file_name] [path/dir_name]: moves file from specified path to specified directory");
        Console.WriteLine("rename [path] [new_name]: renames item on specified path");
        Console.WriteLine("echo [path/.../file_name] [user_input]: writes user input to specified file in path");
        Console.WriteLine("cat [path/.../file_name]: prints file data from path");
        Console.WriteLine("rm(-r) [file/dir_name]: deletes file or directory content with option to flush whole directory");
        Console.WriteLine("stat [path/.../file_name]: prints file attributes and corresponding i-node information");
        Console.WriteLine("dstat: prints current disk stats");
        Console.WriteLine("help: prints all available commands with corresponding explanations");
        Console.WriteLine("clear: clears text from the console window");
        Console.WriteLine("exit: quits application\n");
    }

    public void Start()
    {
        StartMessage();
        bool flag;

        while (true)
        {
            flag = false;
            do
            {
                Console.Write("command> ");
                userInput = Console.ReadLine();
                string trimmedInput = userInput.Trim();
                flag = Operate(trimmedInput);
            }
            while (flag == false);
        }
    }

    public static void ErrorMessage(string message, string arg1 = null, string arg2 = null)
    {
        Console.ForegroundColor = ConsoleColor.Red;

        if (arg1 == null && arg2 == null)
        {
            Console.WriteLine(message);
        }
        else if (arg1 != null && arg2 == null)
        {
            Console.WriteLine(message, arg1);
        }
        else
        {
            Console.WriteLine(message, arg1, arg2);
        }

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine();
    }

    private void StartMessage()
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("Type in \"help\" to view available commands\n");
        Console.ForegroundColor = ConsoleColor.White;
    }

    private void PrintPathExample()
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("Input path examle: ROOT/DIR1/file1, or, ROOT/file1\n");
        Console.ForegroundColor = ConsoleColor.White;
    }

    public enum InputScenario : byte
    {
        SingleString = 1,
        DoubleString = 2,
        TripleString = 3,
        MultipleString = 4
    }

    private bool IsCommandValid(string command)
    {
        if (availableCommands.Contains(command))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool IsStringInPathFormat(string input)
    {
        if (!input.Contains("/"))
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private int PathDepthLevel(string path)
    {
        int depth = 0;

        for (int i = 0; i < path.Length; i++)
        {
            if (path[i] == '/')
            {
                depth++;
            }
        }

        return depth;
    }

    private bool InspectPath(string path, string command, string depthSupport)
    {
        int depth;

        if (depthSupport.Equals("1"))
        {
            depth = 1;
        }
        else if (depthSupport.Equals("2"))
        {
            depth = 2;
        }
        else if (depthSupport.Equals("3"))
        {
            depth = 3;
        }
        else
        {
            depth = -1;
        }

        if (!IsStringInPathFormat(path))
        {
            Terminal.ErrorMessage("{0} is not in path format", path);
            return false;
        }
        else if (PathDepthLevel(path) > depth)
        {
            Terminal.ErrorMessage("'{0}' supports up to level {1} depth path", command, depthSupport);
            return false;
        }
        else if (!path.Contains("ROOT"))
        {
            Terminal.ErrorMessage("Incorrect path");
            PrintPathExample();
            return false;
        }
        else
        {
            return true;
        }
    }

    public InputScenario DetermineInputScenario(string input)
    {
        string trimmedInput = input.Trim();

        if (!trimmedInput.Contains(" "))
        {
            return InputScenario.SingleString;
        }
        else
        {
            int whiteSpaceCount = 0;

            for (int i = 0; i < trimmedInput.Length; i++)
            {
                if (Char.IsWhiteSpace(trimmedInput[i]))
                {
                    whiteSpaceCount++;
                }
            }

            if (whiteSpaceCount == 1)
            {
                return InputScenario.DoubleString;
            }
            else if (whiteSpaceCount == 2)
            {
                return InputScenario.TripleString;
            }
        }
        return InputScenario.MultipleString;
    }

    public bool Operate(string input) //already trimmed
    {
        if (DetermineInputScenario(input) == InputScenario.SingleString)
        {
            return OperateSingleString(input);
        }
        else if (DetermineInputScenario(input) == InputScenario.DoubleString)
        {
            return OperateDoubleString(input);
        }
        else if (DetermineInputScenario(input) == InputScenario.TripleString)
        {
            return OperateTripleString(input);
        }
        else
        {
            ErrorMessage("Invalid input");
            return false;
        }
    }

    private bool OperateSingleString(string input)
    {
        if (!IsCommandValid(input))
        {
            Terminal.ErrorMessage("'{0}' command was not recognised", input);
            return false;
        }
        else
        {
            if (input.Equals("help"))
            {
                Help();
            }
            else if (input.Equals("clear"))
            {
                Console.Clear();
                StartMessage();
            }
            else if (input.Equals("exit"))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Environment.Exit(1);
            }
            else if (input.Equals("dstat"))
            {
                disk.PrintDiskStats();
            }
            else
            {
                ErrorMessage("'{0}' command requires arguments", input);
            }

            return true;
        }
    }

    private bool OperateTripleString(string input)
    {
        string[] keys = input.Split(" "), path1El, path2El;
        string path1 = "", path2 = "";
        string command = keys[0];
        int numberOfPaths = 0;

        if (!IsCommandValid(command))
        {
            ErrorMessage("'{0}' command was not recognised", command);
            return false;
        }

        if (keys[1].Contains("/") && keys[2].Contains("/"))
        {
            path1 = keys[1];
            path2 = keys[2];
            path1El = path1.Split("/");
            path2El = path2.Split("/");
            numberOfPaths = 2;
        }
        else if (keys[1].Contains("/") && !keys[2].Contains("/"))
        {
            path1 = keys[1];
            path1El = path1.Split("/");
            path2 = null;
            path2El = null;
            numberOfPaths = 1;
        }
        else
        {
            ErrorMessage("Invalid input");
            return false;
        }

        if (command.Equals("echo") && numberOfPaths == 1 && InspectPath(path1, command, "2"))
        {
            //echo input u terminalu ne smije sadrzavati white space-s inace terminal nece raditi
            disk.InsertDataToFile(keys[2], path1); //if input data contains "/", it wont be accepted. fix this
            return true;
        }
        else if (command.Equals("rename") && numberOfPaths == 1 && InspectPath(path1, command, "2"))
        {
            disk.Rename(path1, keys[2]);
            return true;
        }
        else if (command.Equals("cp") && numberOfPaths == 2 && InspectPath(path1, command, "2") && InspectPath(path2, command, "2"))
        {
            if (path1El.Length == 3 && path2.Equals("ROOT/"))
            {
                disk.CopyFile(path1El[2], path1El[1], "ROOT");
                return true;
            }
            else if (path1El.Length == 2 && path2El.Length == 2)
            {
                disk.CopyFile(path1El[1], path1El[0], path2El[1]);
                return true;
            }
            else if (path1El.Length == 3 && path2El.Length == 2)
            {
                disk.CopyFile(path1El[2], path1El[1], path2El[1]);
                return true;
            }
        }
        else if (command.Equals("mv") && numberOfPaths == 2 && InspectPath(path1, command, "2") && InspectPath(path2, command, "2"))
        {
            if (path1El.Length == 3 && path2.Equals("ROOT/"))
            {
                disk.MoveFile(path1El[2], path1El[1], "ROOT");
                return true;
            }
            else if (path1El.Length == 2 && path2El.Length == 2)
            {
                disk.MoveFile(path1El[1], path1El[0], path2El[1]);
                return true;
            }
            else if (path1El.Length == 3 && path2El.Length == 2)
            {
                disk.MoveFile(path1El[2], path1El[1], path2El[1]);
                return true;
            }
        }
        else if (command.Equals("clear") || command.Equals("exit") || command.Equals("dstat") || command.Equals("help"))
        {
            ErrorMessage("'{0}' command does not take any arguments", command);
            return false;
        }
        else if (command.Equals("mkdir") || command.Equals("create") || command.Equals("put") || command.Equals("get") || command.Equals("ls")
            || command.Equals("cat") || command.Equals("rm") || command.Equals("rm-r") || command.Equals("stat"))
        {
            ErrorMessage("'{0}' command took too many arguments", command);
            return false;
        }

        return false;
    }

    private bool OperateDoubleString(string input)
    {
        string[] keys = input.Split(" "), pathElements;
        string command = keys[0];
        string path = keys[1];
        pathElements = keys[1].Split("/");

        if (!IsCommandValid(command))
        {
            ErrorMessage("'{0}' command was not recognised", command);
            return false;
        }
        else if (command.Equals("mkdir") && InspectPath(path, command, "1"))
        {
            if (pathElements[1].Equals("ROOT"))
            {
                ErrorMessage("ROOT name is not available for use");
                return false;
            }
            else if (pathElements[1].Length > 15)
            {
                ErrorMessage("Directory name is too long. Try something with 15 characters or less");
                return false;
            }
            else if (createdSubDirectories.Count == 8)
            {
                ErrorMessage("Maximum directory count has been reached (8). Adding failed");
                return false;
            }
            else if (createdSubDirectories.Contains(pathElements[1]))
            {
                ErrorMessage("Directory with the same name already exists. Please enter another name");
                return false;
            }
            else
            {
                disk.AddIDirNode(pathElements[1]);
                createdSubDirectories.AddLast(pathElements[1]);
                return true;
            }
        }
        else if (command.Equals("cat") && InspectPath(path, command, "2"))
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            disk.DisplayFileContent(path);
            Console.ForegroundColor = ConsoleColor.White;
            return true;
        }
        else if (command.Equals("stat") && InspectPath(path, command, "2"))
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("-------------------------------");
            disk.PrintFileStats(path);
            Console.WriteLine("\n-------------------------------");
            Console.ForegroundColor = ConsoleColor.White;
            return true;
        }
        else if (command.Equals("create") && InspectPath(path, command, "2"))
        {
            disk.AddIFileNode(path);
            return true;
        }
        else if (command.Equals("ls") && InspectPath(path, command, "2")) //ls ROOT/
        {
            Console.WriteLine("---------------");
            if (path.Equals("ROOT/"))
            {
                disk.ListPathContent(path.Replace("/", String.Empty));
            }
            else
            {
                disk.ListPathContent(path);
            }

            Console.WriteLine("---------------\n");
            return true;
        }
        else if (command.Equals("rm") && InspectPath(path, command, "2"))
        {
            if (pathElements.Length == 3)
            {
                disk.DeleteFileInDIR(pathElements[2], pathElements[1]);
            }
            else
            {
                if (createdSubDirectories.Contains(pathElements[1]))
                {
                    disk.DeleteDirectory(pathElements[1]);
                    createdSubDirectories.Remove(pathElements[1]);
                }
                else
                {
                    disk.DeleteFileInDIR(pathElements[1]);
                }
            }
        }
        else if (command.Equals("rm-r") && InspectPath(path, command, "2"))
        {
            if (path.Equals("ROOT/"))
            {
                disk.FlushRootDirectory();
                createdSubDirectories.Clear();
            }
            else if (createdSubDirectories.Contains(pathElements[1]))
            {
                disk.DeleteAllFiles(pathElements[1]);
            }

            return true;
        }
        else if (command.Equals("put"))
        {
            try
            {
                disk.UploadFileToRoot(keys[1]);
                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage(ex.Message);
                return false;
            }
        }
        else if (command.Equals("get") && InspectPath(path, command, "2"))
        {
            try
            {
                if (pathElements.Length == 3)
                {
                    disk.DownloadFileFromDirectory(pathElements[2], pathElements[1]);
                }
                else if (pathElements.Length == 2)
                {
                    disk.DownloadFileFromDirectory(pathElements[1], pathElements[0]);
                }

                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage(ex.Message);
                return false;
            }
        }
        else if (command.Equals("clear") || command.Equals("exit") || command.Equals("dstat") || command.Equals("help"))
        {
            ErrorMessage("'{0}' command does not take any arguments", command);
            return false;
        }
        else if (command.Equals("rename") || command.Equals("echo") || command.Equals("mv") || command.Equals("cp"))
        {
            ErrorMessage("'{0}' command reqires more arguments", command);
            return false;
        }

        return false;
    }
}