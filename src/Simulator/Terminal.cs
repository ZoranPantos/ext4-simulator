using Ext4FileSystemSimulation.Enums;
using Ext4FileSystemSimulation.Strategies.CommandStrategies;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ext4FileSystemSimulation;

/// <summary>
/// The interactive shell: reads a line, classifies it by argument count, and
/// dispatches to the matching command strategy.
/// </summary>
internal sealed class Terminal
{
    private readonly ITerminalContext _context;
    private readonly Dictionary<InputScenario, ICommandStrategy> _strategies;

    private string userInput;

    public Terminal(IEnumerable<ICommandStrategy> strategies, ITerminalContext context)
    {
        ArgumentNullException.ThrowIfNull(strategies);

        _context = context ?? throw new ArgumentNullException(nameof(context));
        _strategies = strategies.ToDictionary(strategy => strategy.Scenario);
    }

    public void Start()
    {
        _context.ShowStartMessage();
        bool flag;

        while (true)
        {
            do
            {
                Console.Write("command> ");
                userInput = Console.ReadLine();

                string trimmedInput = userInput.Trim();
                flag = Operate(trimmedInput);
            }
            while (!flag);
        }
    }

    public bool Operate(string input)
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
            2 => InputScenario.TwoArguments,
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
}