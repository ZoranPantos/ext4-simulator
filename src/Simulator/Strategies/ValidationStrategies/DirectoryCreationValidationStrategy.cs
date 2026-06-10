using Ext4FileSystemSimulation.Strategies.CommandStrategies;
using System;

namespace Ext4FileSystemSimulation.Strategies.ValidationStrategies;

/// <summary>
/// Validates the 'mkdir' command: the path (depth 1), the reserved ROOT name,
/// the 15-character name limit, the 8-directory cap, and duplicate names.
/// </summary>
internal sealed class DirectoryCreationValidationStrategy : IValidationStrategy
{
    private readonly ITerminalContext _context;

    public DirectoryCreationValidationStrategy(ITerminalContext context) =>
        _context = context ?? throw new ArgumentNullException(nameof(context));

    public bool IsValid(string[] keys)
    {
        if (!_context.InspectPath(keys[1], keys[0], "1"))
            return false;

        string directoryName = keys[1].Split("/")[1];

        if (directoryName.Equals("ROOT"))
        {
            Terminal.ErrorMessage("ROOT name is not available for use");
            return false;
        }

        if (directoryName.Length > 15)
        {
            Terminal.ErrorMessage("Directory name is too long. Try something with 15 characters or less");
            return false;
        }

        if (_context.CreatedSubDirectories.Count == 8)
        {
            Terminal.ErrorMessage("Maximum directory count has been reached (8). Adding failed");
            return false;
        }

        if (_context.CreatedSubDirectories.Contains(directoryName))
        {
            Terminal.ErrorMessage("Directory with the same name already exists. Please enter another name");
            return false;
        }

        return true;
    }
}