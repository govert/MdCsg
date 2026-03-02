using System;
using System.Collections.Generic;
using System.Linq;
using MdCsg.Api;
using MdCsg.Robust;

namespace MdCsg.Showcase;

internal static class ShowcaseRuntimeOptions
{
    public static bool UseLegacyCsg { get; set; }
    public static bool AllowLegacyFailover { get; set; }
    public static bool UseClosureAttemptRobust { get; set; } = true;
}

internal static class ShowcaseCsg
{
    public static Solid Union(Solid a, Solid b)
        => Execute(
            "Union",
            () => RobustCsg.Union(a, b, BuildRobustOptions()),
            () => Csg.Union(a, b, new CsgOptions { Parallel = false }));

    public static Solid Intersect(Solid a, Solid b)
        => Execute(
            "Intersect",
            () => RobustCsg.Intersect(a, b, BuildRobustOptions()),
            () => Csg.Intersect(a, b, new CsgOptions { Parallel = false }));

    public static Solid Difference(Solid a, Solid b)
        => Execute(
            "Difference",
            () => RobustCsg.Difference(a, b, BuildRobustOptions()),
            () => Csg.Difference(a, b, new CsgOptions { Parallel = false }));

    private static Solid Execute(
        string opName,
        Func<RobustCsgResult> runRobust,
        Func<CsgResult> runLegacy)
    {
        if (ShowcaseRuntimeOptions.UseLegacyCsg)
            return new Solid(runLegacy().Mesh);

        var robust = runRobust();
        if (robust.Succeeded && robust.Result is not null)
            return new Solid(robust.Result.Mesh);

        string issues = BuildIssueSummary(robust.Issues);
        int closureAttempt = ShowcaseRuntimeOptions.UseClosureAttemptRobust ? 1 : 0;

        if (!ShowcaseRuntimeOptions.AllowLegacyFailover)
        {
            throw new InvalidOperationException(
                $"[showcase] robust {opName} failed in strict mode (closureAttempt={closureAttempt}). "
                + "Use --legacy-csg for full legacy mode or --allow-legacy-failover for fallback. "
                + $"Issues: {issues}");
        }

        Console.WriteLine(
            $"[showcase] robust {opName} failed (closureAttempt={closureAttempt}); using explicit legacy failover. Issues: {issues}");
        return new Solid(runLegacy().Mesh);
    }

    private static RobustOperationOptions BuildRobustOptions()
        => new()
        {
            Mode = RobustMode.Strict,
            Deterministic = true,
            UseRobustTriangulationKernel = true,
            AttemptResidualDegenerateClosure = ShowcaseRuntimeOptions.UseClosureAttemptRobust
        };

    internal static string BuildIssueSummary(IReadOnlyList<RobustIssue> issues)
        => issues.Count == 0
            ? "<none>"
            : string.Join(
                " | ",
                issues
                    .Distinct()
                    .OrderBy(static i => SeverityOrder(i.Severity))
                    .ThenBy(static i => i.Code)
                    .ThenBy(static i => i.Message, StringComparer.Ordinal)
                    .Select(static i => $"{i.Severity}:{i.Code}:{i.Message}"));

    private static int SeverityOrder(RobustIssueSeverity severity)
        => severity switch
        {
            RobustIssueSeverity.Error => 0,
            RobustIssueSeverity.Warning => 1,
            RobustIssueSeverity.Info => 2,
            _ => 3
        };
}
