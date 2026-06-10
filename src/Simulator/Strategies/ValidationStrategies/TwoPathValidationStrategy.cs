using Ext4FileSystemSimulation.Strategies.CommandStrategies;
using System;

namespace Ext4FileSystemSimulation.Strategies.ValidationStrategies;

/// <summary>
/// Validates two path arguments (keys[1] and keys[2]), each up to depth 2.
/// Used by the two-path commands (cp, mv).
/// </summary>
internal sealed class TwoPathValidationStrategy : IValidationStrategy
{
    private readonly ITerminalContext _context;

    public TwoPathValidationStrategy(ITerminalContext context) =>
        _context = context ?? throw new ArgumentNullException(nameof(context));

    public bool IsValid(string[] keys)
        => _context.InspectPath(keys[1], keys[0], "2") && _context.InspectPath(keys[2], keys[0], "2");
}