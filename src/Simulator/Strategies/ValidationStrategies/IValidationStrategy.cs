namespace Ext4FileSystemSimulation.Strategies.ValidationStrategies;

/// <summary>
/// Validates a parsed command line (already split on spaces) before its command
/// is executed. An implementation emits its own error message and returns false
/// when validation fails, mirroring the original inline checks.
/// </summary>
internal interface IValidationStrategy
{
    bool IsValid(string[] keys);
}