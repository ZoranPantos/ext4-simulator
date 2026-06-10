using System;
using Ext4FileSystemSimulation.Strategies.ValidationStrategies;

namespace Ext4FileSystemSimulation.Strategies.CommandStrategies;

/// <summary>
/// Strategy for commands that take a single argument: mkdir, create, cat, stat,
/// ls, rm, rm-r, put, get.
/// </summary>
internal sealed class OneArgumentCommandStrategy : ICommandStrategy
{
    private readonly ITerminalContext _context;
    private readonly IValidationStrategy _pathValidator;
    private readonly IValidationStrategy _directoryCreationValidator;

    public OneArgumentCommandStrategy(ITerminalContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));

        _pathValidator = new PathValidationStrategy(context, "2");
        _directoryCreationValidator = new DirectoryCreationValidationStrategy(context);
    }

    public bool Handle(string input)
    {
        string[] keys = input.Split(" ");
        string command = keys[0];
        string path = keys[1];
        string[] pathElements = keys[1].Split("/");

        if (!_context.IsCommandValid(command))
        {
            Terminal.ErrorMessage("'{0}' command was not recognised", command);
            return false;
        }
        else if (command.Equals("mkdir") && _directoryCreationValidator.IsValid(keys))
        {
            _context.Storage.AddDirNode(pathElements[1]);
            _context.CreatedSubDirectories.Add(pathElements[1]);

            return true;
        }
        else if (command.Equals("cat") && _pathValidator.IsValid(keys))
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            _context.Storage.DisplayFileContent(path);
            Console.ForegroundColor = ConsoleColor.White;

            return true;
        }
        else if (command.Equals("stat") && _pathValidator.IsValid(keys))
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("-------------------------------");
            _context.Storage.PrintFileStats(path);
            Console.WriteLine("\n-------------------------------");
            Console.ForegroundColor = ConsoleColor.White;

            return true;
        }
        else if (command.Equals("create") && _pathValidator.IsValid(keys))
        {
            _context.Storage.AddFileNode(path);
            return true;
        }
        else if (command.Equals("ls") && _pathValidator.IsValid(keys)) //ls ROOT/
        {
            Console.WriteLine("---------------");

            if (path.Equals("ROOT/"))
                _context.Storage.ListPathContent(path.Replace("/", string.Empty));
            else
                _context.Storage.ListPathContent(path);

            Console.WriteLine("---------------\n");

            return true;
        }
        else if (command.Equals("rm") && _pathValidator.IsValid(keys))
        {
            if (pathElements.Length == 3)
                _context.Storage.DeleteFileInDIR(pathElements[2], pathElements[1]);
            else
            {
                if (_context.CreatedSubDirectories.Contains(pathElements[1]))
                {
                    _context.Storage.DeleteDirectory(pathElements[1]);
                    _context.CreatedSubDirectories.Remove(pathElements[1]);
                }
                else
                    _context.Storage.DeleteFileInDIR(pathElements[1]);
            }
        }
        else if (command.Equals("rm-r") && _pathValidator.IsValid(keys))
        {
            if (path.Equals("ROOT/"))
            {
                _context.Storage.FlushRootDirectory();
                _context.CreatedSubDirectories.Clear();
            }
            else if (_context.CreatedSubDirectories.Contains(pathElements[1]))
                _context.Storage.DeleteAllFiles(pathElements[1]);

            return true;
        }
        else if (command.Equals("put"))
        {
            try
            {
                _context.Storage.UploadFileToRoot(keys[1]);
                return true;
            }
            catch (Exception ex)
            {
                Terminal.ErrorMessage(ex.Message);
                return false;
            }
        }
        else if (command.Equals("get") && _pathValidator.IsValid(keys))
        {
            try
            {
                if (pathElements.Length == 3)
                    _context.Storage.DownloadFileFromDirectory(pathElements[2], pathElements[1]);
                else if (pathElements.Length == 2)
                    _context.Storage.DownloadFileFromDirectory(pathElements[1], pathElements[0]);

                return true;
            }
            catch (Exception ex)
            {
                Terminal.ErrorMessage(ex.Message);
                return false;
            }
        }
        else if (command.Equals("clear") || command.Equals("exit") || command.Equals("dstat") || command.Equals("help"))
        {
            Terminal.ErrorMessage("'{0}' command does not take any arguments", command);
            return false;
        }
        else if (command.Equals("rename") || command.Equals("echo") || command.Equals("mv") || command.Equals("cp"))
        {
            Terminal.ErrorMessage("'{0}' command reqires more arguments", command);
            return false;
        }

        return false;
    }
}
