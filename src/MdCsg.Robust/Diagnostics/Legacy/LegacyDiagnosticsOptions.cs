namespace MdCsg.Robust.Diagnostics.Legacy;

/// <summary>
/// Explicit opt-in options for diagnostics-only legacy comparisons.
/// Strict robust consumers should not enable legacy execution in production flows.
/// </summary>
public sealed class LegacyDiagnosticsOptions
{
    public bool AllowLegacyExecution { get; init; }

    public RobustOperationOptions? RobustOptions { get; init; }
}
