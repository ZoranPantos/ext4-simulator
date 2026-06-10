using Ext4FileSystemSimulation.Enums;
using System;

namespace Ext4FileSystemSimulation.Strategies.CommandStrategies;

/// <summary>
/// Strategy for bare commands that take no arguments: help, clear, exit, dstat.
/// </summary>
internal sealed class NoArgumentCommandStrategy : ICommandStrategy
{
    private readonly ITerminalContext _context;

    public NoArgumentCommandStrategy(ITerminalContext context) =>
        _context = context ?? throw new ArgumentNullException(nameof(context));

    public InputScenario Scenario => InputScenario.NoArgument;

    public bool Handle(string input)
    {
        if (!_context.IsCommandValid(input))
        {
            Terminal.ErrorMessage("'{0}' command was not recognised", input);
            return false;
        }

        switch (input)
        {
            case "help":
                Help();
                break;
            case "clear":
                Console.Clear();
                _context.ShowStartMessage();
                break;
            case "exit":
                Console.ForegroundColor = ConsoleColor.Green;
                Environment.Exit(1);
                break;
            case "dstat":
                _context.Storage.PrintDiskStats();
                break;
            default:
                Terminal.ErrorMessage("'{0}' command requires arguments", input);
                break;
        }

        return true;
    }

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
}