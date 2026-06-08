namespace Ext4FileSystemSimulation.CommandStrategies;

/// <summary>
/// Strategy for handling a single trimmed command line. The concrete strategy is
/// chosen by <see cref="Terminal"/> based on how many arguments the input has.
/// </summary>
internal interface ICommandStrategy
{
    /// <summary>
    /// Processes the command line. Returns the same flag the original
    /// Operate* methods returned (drives the terminal's input loop).
    /// </summary>
    bool Handle(string input);
}
