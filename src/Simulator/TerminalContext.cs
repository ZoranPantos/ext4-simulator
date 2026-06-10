using Ext4FileSystemSimulation.Strategies.CommandStrategies;
using System;
using System.Collections.Generic;

namespace Ext4FileSystemSimulation;

/// <summary>
/// Default <see cref="ITerminalContext"/> implementation. Owns the command
/// catalogue and the in-memory directory registry, and provides the shared
/// path/command validation and start-message helpers the command strategies
/// depend on. Has no reference back to <see cref="Terminal"/>.
/// </summary>
internal sealed class TerminalContext : ITerminalContext
{
    private readonly SystemStorage _systemStorage;

    private readonly LinkedList<string> _availableCommands = new(
    [
        "mkdir", "create", "put", "get", "ls", "cp", "mv", "rename", "echo",
        "cat", "rm", "rm-r", "stat", "dstat", "help", "clear", "exit"
    ]);

    private readonly LinkedList<string> _createdSubDirectories = [];

    public TerminalContext(SystemStorage systemStorage)
        => _systemStorage = systemStorage ?? throw new ArgumentNullException(nameof(systemStorage));

    public SystemStorage Storage => _systemStorage;

    public ICollection<string> CreatedSubDirectories => _createdSubDirectories;

    public bool IsCommandValid(string command) => _availableCommands.Contains(command);

    public bool InspectPath(string path, string command, string depthSupport)
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
            return true;
    }

    public void ShowStartMessage()
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("Type in \"help\" to view available commands\n");
        Console.ForegroundColor = ConsoleColor.White;
    }

    private static bool IsStringInPathFormat(string input) => input.Contains('/');

    private static int PathDepthLevel(string path)
    {
        int depth = 0;

        for (int i = 0; i < path.Length; i++)
        {
            if (path[i].Equals('/'))
                depth++;
        }

        return depth;
    }

    private static void PrintPathExample()
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("Input path examle: ROOT/DIR1/file1, or, ROOT/file1\n");
        Console.ForegroundColor = ConsoleColor.White;
    }
}