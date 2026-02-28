using System;
using System.Linq;
using MdCsg.Api;
using MdCsg.Robust;

namespace MdCsg.Showcase;

internal static class ShowcaseRuntimeOptions
{
    public static bool UseLegacyCsg { get; set; }
}

internal static class ShowcaseCsg
{
    private static readonly RobustOperationOptions StrictRobustOptions = new()
    {
        Mode = RobustMode.Strict,
        Deterministic = true,
        UseRobustTriangulationKernel = true
    };

    public static Solid Union(Solid a, Solid b)
        => Execute(
            "Union",
            () => RobustCsg.Union(a, b, StrictRobustOptions),
            () => Csg.Union(a, b, new CsgOptions { Parallel = false }));

    public static Solid Intersect(Solid a, Solid b)
        => Execute(
            "Intersect",
            () => RobustCsg.Intersect(a, b, StrictRobustOptions),
            () => Csg.Intersect(a, b, new CsgOptions { Parallel = false }));

    public static Solid Difference(Solid a, Solid b)
        => Execute(
            "Difference",
            () => RobustCsg.Difference(a, b, StrictRobustOptions),
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

        string issues = robust.Issues.Count == 0
            ? "<none>"
            : string.Join(
                " | ",
                robust.Issues.Select(static i => $"{i.Severity}:{i.Code}:{i.Message}"));
        Console.WriteLine($"[showcase] robust {opName} failed; falling back to legacy. Issues: {issues}");
        return new Solid(runLegacy().Mesh);
    }
}
