using System.Collections.Generic;

namespace Ext4FileSystemSimulation.Strategies.CommandStrategies;

/// <summary>
/// The terminal services a command strategy needs: the backing storage, the
/// in-memory list of created sub-directories, and the shared validation/UI
/// helpers. <see cref="Terminal"/> provides the implementation.
/// </summary>
internal interface ITerminalContext
{
    ISystemStorage Storage { get; }
    ICollection<string> CreatedSubDirectories { get; }

    bool IsCommandValid(string command);
    bool InspectPath(string path, string command, string depthSupport);
    void ShowStartMessage();
}