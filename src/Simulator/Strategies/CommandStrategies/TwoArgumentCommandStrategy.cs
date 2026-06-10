using Ext4FileSystemSimulation.Enums;
using Ext4FileSystemSimulation.Strategies.ValidationStrategies;
using System;

namespace Ext4FileSystemSimulation.Strategies.CommandStrategies;

/// <summary>
/// Strategy for commands that take two arguments: echo, rename, cp, mv.
/// </summary>
internal sealed class TwoArgumentCommandStrategy : ICommandStrategy
{
    private readonly ITerminalContext _context;
    private readonly IValidationStrategy _pathValidator;
    private readonly IValidationStrategy _twoPathValidator;

    public TwoArgumentCommandStrategy(
        ITerminalContext context,
        PathValidationStrategy pathValidator,
        TwoPathValidationStrategy twoPathValidator)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _pathValidator = pathValidator ?? throw new ArgumentNullException(nameof(pathValidator));
        _twoPathValidator = twoPathValidator ?? throw new ArgumentNullException(nameof(twoPathValidator));
    }

    public InputScenario Scenario => InputScenario.TwoArguments;

    public bool Handle(string input)
    {
        string[] keys = input.Split(" ");
        string[] path1El;
        string[] path2El;
        string path1 = "";
        string path2 = "";
        string command = keys[0];
        int numberOfPaths = 0;

        if (!_context.IsCommandValid(command))
        {
            Terminal.ErrorMessage("'{0}' command was not recognised", command);
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
            Terminal.ErrorMessage("Invalid input");
            return false;
        }

        if (command.Equals("echo") && numberOfPaths == 1 && _pathValidator.IsValid(keys))
        {
            _context.Storage.InsertDataToFile(keys[2], path1);
            return true;
        }
        else if (command.Equals("rename") && numberOfPaths == 1 && _pathValidator.IsValid(keys))
        {
            _context.Storage.Rename(path1, keys[2]);
            return true;
        }
        else if (command.Equals("cp") && numberOfPaths == 2 && _twoPathValidator.IsValid(keys))
        {
            if (path1El.Length == 3 && path2.Equals("ROOT/"))
            {
                _context.Storage.CopyFile(path1El[2], path1El[1], "ROOT");
                return true;
            }
            else if (path1El.Length == 2 && path2El.Length == 2)
            {
                _context.Storage.CopyFile(path1El[1], path1El[0], path2El[1]);
                return true;
            }
            else if (path1El.Length == 3 && path2El.Length == 2)
            {
                _context.Storage.CopyFile(path1El[2], path1El[1], path2El[1]);
                return true;
            }
        }
        else if (command.Equals("mv") && numberOfPaths == 2 && _twoPathValidator.IsValid(keys))
        {
            if (path1El.Length == 3 && path2.Equals("ROOT/"))
            {
                _context.Storage.MoveFile(path1El[2], path1El[1], "ROOT");
                return true;
            }
            else if (path1El.Length == 2 && path2El.Length == 2)
            {
                _context.Storage.MoveFile(path1El[1], path1El[0], path2El[1]);
                return true;
            }
            else if (path1El.Length == 3 && path2El.Length == 2)
            {
                _context.Storage.MoveFile(path1El[2], path1El[1], path2El[1]);
                return true;
            }
        }
        else if (command.Equals("clear") || command.Equals("exit") || command.Equals("dstat") || command.Equals("help"))
        {
            Terminal.ErrorMessage("'{0}' command does not take any arguments", command);
            return false;
        }
        else if (command.Equals("mkdir") || command.Equals("create") || command.Equals("put") || command.Equals("get") || command.Equals("ls")
            || command.Equals("cat") || command.Equals("rm") || command.Equals("rm-r") || command.Equals("stat"))
        {
            Terminal.ErrorMessage("'{0}' command took too many arguments", command);
            return false;
        }

        return false;
    }
}