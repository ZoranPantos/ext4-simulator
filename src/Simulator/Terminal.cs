using System;
using System.Collections.Generic;

namespace Ext4FileSystemSimulation;

public class Terminal
{
    private readonly SystemStorage _systemStorage;

    private readonly LinkedList<string> _availableCommands = new(
    [
        "mkdir", "create", "put", "get", "ls", "cp", "mv", "rename", "echo",
        "cat", "rm", "rm-r", "stat", "dstat", "help", "clear", "exit"
    ]);

    private readonly LinkedList<string> _createdSubDirectories = [];
    private string userInput;

    public Terminal(SystemStorage systemStorage) =>
        _systemStorage = systemStorage ?? throw new ArgumentNullException(nameof(systemStorage));

    private void Help()
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(
            """

            AVAILABLE COMMANDS
            (paths begin at ROOT and use '/', e.g. ROOT/file or ROOT/dir/file)

              mkdir ROOT/<dir>            create a directory (max 8, name<=15)
              create ROOT/<file>          create an empty file (also in a dir)
              echo ROOT/<file> <text>     write <text> into a file (no spaces)
              cat ROOT/<file>             print a file's contents
              ls ROOT/                    list ROOT (a dir: ls ROOT/<dir>)
              stat ROOT/<file>            show file attributes + i-node info
              rename ROOT/<item> <name>   rename a file or directory
              cp ROOT/<file> <dir>        copy a file into a directory
              mv ROOT/<file> <dir>        move a file into a directory
              rm ROOT/<file>              delete a file or a directory
              rm-r ROOT/                  empty ROOT or a directory
              put <host_file>             upload a host file into ROOT
              get ROOT/<file>             download a file to host folder
              dstat                       print disk statistics
              help                        show this help
              clear                       clear the screen
              exit                        quit the application

            Notes:
              - only one directory level (ROOT/dir); ROOT always exists
              - <text>/<name> take a single token (no spaces, no '/')
              - cp/mv into ROOT: write the destination as 'ROOT/'
              - get will not overwrite a file that already exists on the host

            """);

        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine(
            """
            EXAMPLES (run them top to bottom):

              mkdir ROOT/docs               create directory 'docs'
              create ROOT/notes.txt         create file 'notes.txt'
              echo ROOT/notes.txt hello     write 'hello' into notes.txt
              cat ROOT/notes.txt            prints: hello
              create ROOT/docs/todo.txt     create 'todo.txt' in 'docs'
              echo ROOT/docs/todo.txt task  write 'task' into todo.txt
              ls ROOT/                      list ROOT
              ls ROOT/docs                  list files in 'docs'
              cp ROOT/notes.txt ROOT/docs   copy notes.txt into 'docs'
              mv ROOT/docs/todo.txt ROOT/   move todo.txt to ROOT
              rename ROOT/notes.txt n.txt   rename to n.txt
              stat ROOT/n.txt               show n.txt attributes
              rm ROOT/n.txt                 delete file n.txt
              rm ROOT/docs                  delete 'docs' + contents
              dstat                         show disk usage

            """);

        Console.ForegroundColor = ConsoleColor.White;
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

        if (arg1 is null && arg2 is null)
            Console.WriteLine(message);
        else if (arg1 is not null && arg2 is null)
            Console.WriteLine(message, arg1);
        else
            Console.WriteLine(message, arg1, arg2);

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

    private bool IsCommandValid(string command) => _availableCommands.Contains(command);

    private bool IsStringInPathFormat(string input) => input.Contains('/');

    private int PathDepthLevel(string path)
    {
        int depth = 0;

        for (int i = 0; i < path.Length; i++)
        {
            if (path[i].Equals('/'))
                depth++;
        }

        return depth;
    }

    private bool InspectPath(string path, string command, string depthSupport)
    {
        int depth = depthSupport switch
        {
            "1" => 1,
            "2" => 2,
            "3" => 3,
            _ => -1
        };

        if (!IsStringInPathFormat(path))
        {
            ErrorMessage("{0} is not in path format", path);
            return false;
        }
        else if (PathDepthLevel(path) > depth)
        {
            ErrorMessage("'{0}' supports up to level {1} depth path", command, depthSupport);
            return false;
        }
        else if (!path.Contains("ROOT"))
        {
            ErrorMessage("Incorrect path");
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

        if (!trimmedInput.Contains(' '))
            return InputScenario.SingleString;
        
        int whiteSpaceCount = 0;

        for (int i = 0; i < trimmedInput.Length; i++)
        {
            if (char.IsWhiteSpace(trimmedInput[i]))
                whiteSpaceCount++;
        }

        return whiteSpaceCount switch
        {
            1 => InputScenario.DoubleString,
            2 => InputScenario.TripleString,
            _ => InputScenario.MultipleString
        };
    }

    public bool Operate(string input) //already trimmed
    {
        switch (DetermineInputScenario(input))
        {
            case InputScenario.SingleString:
                return OperateSingleString(input);
            case InputScenario.DoubleString:
                return OperateDoubleString(input);
            case InputScenario.TripleString:
                return OperateTripleString(input);
            default:
                ErrorMessage("Invalid input");
                return false;
        }
    }

    private bool OperateSingleString(string input)
    {
        if (!IsCommandValid(input))
        {
            ErrorMessage("'{0}' command was not recognised", input);
            return false;
        }

        switch (input)
        {
            case "help":
                Help();
                break;
            case "clear":
                Console.Clear();
                StartMessage();
                break;
            case "exit":
                Console.ForegroundColor = ConsoleColor.Green;
                Environment.Exit(1);
                break;
            case "dstat":
                _systemStorage.PrintDiskStats();
                break;
            default:
                ErrorMessage("'{0}' command requires arguments", input);
                break;
        }

        return true;
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

        if (keys[1].Contains('/') && keys[2].Contains('/'))
        {
            path1 = keys[1];
            path2 = keys[2];
            path1El = path1.Split("/");
            path2El = path2.Split("/");
            numberOfPaths = 2;
        }
        else if (keys[1].Contains('/') && !keys[2].Contains('/'))
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
            _systemStorage.InsertDataToFile(keys[2], path1); //if input data contains "/", it wont be accepted. fix this
            return true;
        }
        else if (command.Equals("rename") && numberOfPaths == 1 && InspectPath(path1, command, "2"))
        {
            _systemStorage.Rename(path1, keys[2]);
            return true;
        }
        else if (command.Equals("cp") && numberOfPaths == 2 && InspectPath(path1, command, "2") && InspectPath(path2, command, "2"))
        {
            if (path1El.Length == 3 && path2.Equals("ROOT/"))
            {
                _systemStorage.CopyFile(path1El[2], path1El[1], "ROOT");
                return true;
            }
            else if (path1El.Length == 2 && path2El.Length == 2)
            {
                _systemStorage.CopyFile(path1El[1], path1El[0], path2El[1]);
                return true;
            }
            else if (path1El.Length == 3 && path2El.Length == 2)
            {
                _systemStorage.CopyFile(path1El[2], path1El[1], path2El[1]);
                return true;
            }
        }
        else if (command.Equals("mv") && numberOfPaths == 2 && InspectPath(path1, command, "2") && InspectPath(path2, command, "2"))
        {
            if (path1El.Length == 3 && path2.Equals("ROOT/"))
            {
                _systemStorage.MoveFile(path1El[2], path1El[1], "ROOT");
                return true;
            }
            else if (path1El.Length == 2 && path2El.Length == 2)
            {
                _systemStorage.MoveFile(path1El[1], path1El[0], path2El[1]);
                return true;
            }
            else if (path1El.Length == 3 && path2El.Length == 2)
            {
                _systemStorage.MoveFile(path1El[2], path1El[1], path2El[1]);
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
            else if (_createdSubDirectories.Count == 8)
            {
                ErrorMessage("Maximum directory count has been reached (8). Adding failed");
                return false;
            }
            else if (_createdSubDirectories.Contains(pathElements[1]))
            {
                ErrorMessage("Directory with the same name already exists. Please enter another name");
                return false;
            }
            else
            {
                _systemStorage.AddIDirNode(pathElements[1]);
                _createdSubDirectories.AddLast(pathElements[1]);
                return true;
            }
        }
        else if (command.Equals("cat") && InspectPath(path, command, "2"))
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            _systemStorage.DisplayFileContent(path);
            Console.ForegroundColor = ConsoleColor.White;
            return true;
        }
        else if (command.Equals("stat") && InspectPath(path, command, "2"))
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("-------------------------------");
            _systemStorage.PrintFileStats(path);
            Console.WriteLine("\n-------------------------------");
            Console.ForegroundColor = ConsoleColor.White;
            return true;
        }
        else if (command.Equals("create") && InspectPath(path, command, "2"))
        {
            _systemStorage.AddIFileNode(path);
            return true;
        }
        else if (command.Equals("ls") && InspectPath(path, command, "2")) //ls ROOT/
        {
            Console.WriteLine("---------------");
            if (path.Equals("ROOT/"))
            {
                _systemStorage.ListPathContent(path.Replace("/", String.Empty));
            }
            else
            {
                _systemStorage.ListPathContent(path);
            }

            Console.WriteLine("---------------\n");
            return true;
        }
        else if (command.Equals("rm") && InspectPath(path, command, "2"))
        {
            if (pathElements.Length == 3)
            {
                _systemStorage.DeleteFileInDIR(pathElements[2], pathElements[1]);
            }
            else
            {
                if (_createdSubDirectories.Contains(pathElements[1]))
                {
                    _systemStorage.DeleteDirectory(pathElements[1]);
                    _createdSubDirectories.Remove(pathElements[1]);
                }
                else
                {
                    _systemStorage.DeleteFileInDIR(pathElements[1]);
                }
            }
        }
        else if (command.Equals("rm-r") && InspectPath(path, command, "2"))
        {
            if (path.Equals("ROOT/"))
            {
                _systemStorage.FlushRootDirectory();
                _createdSubDirectories.Clear();
            }
            else if (_createdSubDirectories.Contains(pathElements[1]))
            {
                _systemStorage.DeleteAllFiles(pathElements[1]);
            }

            return true;
        }
        else if (command.Equals("put"))
        {
            try
            {
                _systemStorage.UploadFileToRoot(keys[1]);
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
                    _systemStorage.DownloadFileFromDirectory(pathElements[2], pathElements[1]);
                else if (pathElements.Length == 2)
                    _systemStorage.DownloadFileFromDirectory(pathElements[1], pathElements[0]);

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