using Ext4FileSystemSimulation.Strategies.CommandStrategies;
using System;

namespace Ext4FileSystemSimulation.Strategies.ValidationStrategies;

/// <summary>
/// Validates that the single path argument (keys[1]) is well-formed and within
/// the configured depth. Used by commands whose only check is the path.
/// </summary>
internal sealed class PathValidationStrategy : IValidationStrategy
{
    private readonly ITerminalContext _context;
    private readonly string _depthSupport;

    public PathValidationStrategy(ITerminalContext context, string depthSupport)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _depthSupport = depthSupport;
    }

    public bool IsValid(string[] keys) => _context.InspectPath(keys[1], keys[0], _depthSupport);
}