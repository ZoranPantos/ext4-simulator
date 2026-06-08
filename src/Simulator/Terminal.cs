using System;
using System.Collections.Generic;
using Ext4FileSystemSimulation.CommandStrategies;
using Ext4FileSystemSimulation.Enums;

namespace Ext4FileSystemSimulation;

public class Terminal : ITerminalContext
{
    private readonly SystemStorage _systemStorage;

    private readonly LinkedList<string> _availableCommands = new(
    [
        "mkdir", "create", "put", "get", "ls", "cp", "mv", "rename", "echo",
        "cat", "rm", "rm-r", "stat", "dstat", "help", "clear", "exit"
    ]);

    private readonly LinkedList<string> _createdSubDirectories = [];
    private readonly Dictionary<InputScenario, ICommandStrategy> _strategies;
    private string userInput;

    public Terminal(SystemStorage systemStorage)
    {
        _systemStorage = systemStorage ?? throw new ArgumentNullException(nameof(systemStorage));

        _strategies = new Dictionary<InputScenario, ICommandStrategy>
        {
            [InputScenario.NoArgument] = new NoArgumentCommandStrategy(this),
            [InputScenario.OneArgument] = new OneArgumentCommandStrategy(this),
            [InputScenario.TwoArgument] = new TwoArgumentCommandStrategy(this)
        };
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

    public bool Operate(string input) //already trimmed
    {
        InputScenario scenario = DetermineInputScenario(input);

        if (_strategies.TryGetValue(scenario, out ICommandStrategy strategy))
            return strategy.Handle(input);

        ErrorMessage("Invalid input");
        return false;
    }

    public InputScenario DetermineInputScenario(string input)
    {
        string trimmedInput = input.Trim();

        if (!trimmedInput.Contains(' '))
            return InputScenario.NoArgument;

        int whiteSpaceCount = 0;

        for (int i = 0; i < trimmedInput.Length; i++)
        {
            if (char.IsWhiteSpace(trimmedInput[i]))
                whiteSpaceCount++;
        }

        return whiteSpaceCount switch
        {
            1 => InputScenario.OneArgument,
            2 => InputScenario.TwoArgument,
            _ => InputScenario.TooManyArguments
        };
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

    // ITerminalContext - services exposed to the command strategies.
    SystemStorage ITerminalContext.Storage => _systemStorage;

    ICollection<string> ITerminalContext.CreatedSubDirectories => _createdSubDirectories;

    bool ITerminalContext.IsCommandValid(string command) => IsCommandValid(command);

    bool ITerminalContext.InspectPath(string path, string command, string depthSupport) =>
        InspectPath(path, command, depthSupport);

    void ITerminalContext.ShowStartMessage() => StartMessage();

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
}
