using Ext4FileSystemSimulation.Enums;

namespace Ext4FileSystemSimulation.Strategies.CommandStrategies;

/// <summary>
/// Strategy for handling a single trimmed command line. The concrete strategy is
/// chosen by <see cref="Terminal"/> based on how many arguments the input has.
/// </summary>
internal interface ICommandStrategy
{
    /// <summary>
    /// The input shape this strategy handles; used to build the dispatch map.
    /// </summary>
    InputScenario Scenario { get; }

    /// <summary>
    /// Processes the command line. Returns the same flag the original
    /// Operate* methods returned (drives the terminal's input loop).
    /// </summary>
    bool Handle(string input);
}