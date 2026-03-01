using MdCsg.Api;

namespace MdCsg.Robust.Diagnostics.Legacy;

/// <summary>
/// Diagnostics-only bridge that runs strict robust and legacy CSG side-by-side.
/// Explicit opt-in is required to prevent accidental legacy dependency.
/// </summary>
public static class LegacyComparison
{
    public static LegacyComparisonResult Union(
        Solid a,
        Solid b,
        LegacyDiagnosticsOptions? options = null)
        => Execute(RobustCsgOperation.Union, a, b, options);

    public static LegacyComparisonResult Intersect(
        Solid a,
        Solid b,
        LegacyDiagnosticsOptions? options = null)
        => Execute(RobustCsgOperation.Intersection, a, b, options);

    public static LegacyComparisonResult Difference(
        Solid a,
        Solid b,
        LegacyDiagnosticsOptions? options = null)
        => Execute(RobustCsgOperation.Difference, a, b, options);

    private static LegacyComparisonResult Execute(
        RobustCsgOperation operation,
        Solid a,
        Solid b,
        LegacyDiagnosticsOptions? options)
    {
        var opts = options ?? new LegacyDiagnosticsOptions();
        if (!opts.AllowLegacyExecution)
        {
            throw new InvalidOperationException(
                "Legacy diagnostics execution is disabled. " +
                "Set LegacyDiagnosticsOptions.AllowLegacyExecution=true for explicit diagnostics-only use.");
        }

        RobustOperationOptions robustOptions = opts.RobustOptions ?? RobustOperationOptions.Default;
        var robustResult = operation switch
        {
            RobustCsgOperation.Union => RobustCsg.Union(a, b, robustOptions),
            RobustCsgOperation.Intersection => RobustCsg.Intersect(a, b, robustOptions),
            RobustCsgOperation.Difference => RobustCsg.Difference(a, b, robustOptions),
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
        };

        var legacyResult = operation switch
        {
            RobustCsgOperation.Union => Csg.Union(a, b),
            RobustCsgOperation.Intersection => Csg.Intersect(a, b),
            RobustCsgOperation.Difference => Csg.Difference(a, b),
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
        };

        return new LegacyComparisonResult
        {
            Operation = operation,
            RobustResult = robustResult,
            LegacyResult = legacyResult
        };
    }
}
