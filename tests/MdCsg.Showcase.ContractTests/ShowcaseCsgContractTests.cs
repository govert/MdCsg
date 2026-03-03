using System.IO;
using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Robust;
using MdCsg.Showcase;

namespace MdCsg.Showcase.ContractTests;

public sealed class ShowcaseCsgContractTests : IDisposable
{
    public ShowcaseCsgContractTests()
    {
        ResetRuntimeOptions();
    }

    public void Dispose()
    {
        ResetRuntimeOptions();
    }

    [Fact]
    public void CliParser_DefaultsToStrictClosureAttemptMode()
    {
        var opts = ShowcaseCliOptions.Parse(Array.Empty<string>());

        Assert.False(opts.UseLegacyCsg);
        Assert.False(opts.AllowLegacyFailover);
        Assert.False(opts.StrictRobustOnly);
        Assert.True(opts.UseClosureAttemptRobust);
        Assert.Equal(
            "[showcase] CSG mode: robust-strict (closureAttempt=1, no automatic fallback)",
            opts.ModeSummary);
    }

    [Fact]
    public void CliParser_LegacyMode_ProducesLegacySummary()
    {
        var opts = ShowcaseCliOptions.Parse(new[] { "--legacy-csg" });

        Assert.True(opts.UseLegacyCsg);
        Assert.False(opts.AllowLegacyFailover);
        Assert.False(opts.StrictRobustOnly);
        Assert.Equal("[showcase] CSG mode: legacy (explicit opt-out)", opts.ModeSummary);
    }

    [Fact]
    public void CliParser_StrictRobustOnly_WithFailover_HasClosureAttemptDisabled()
    {
        var opts = ShowcaseCliOptions.Parse(new[] { "--strict-robust-only", "--allow-legacy-failover" });

        Assert.False(opts.UseLegacyCsg);
        Assert.True(opts.AllowLegacyFailover);
        Assert.True(opts.StrictRobustOnly);
        Assert.False(opts.UseClosureAttemptRobust);
        Assert.Equal(
            "[showcase] CSG mode: robust-strict (closureAttempt=0) with explicit legacy failover",
            opts.ModeSummary);
    }

    [Fact]
    public void CliParser_ScreenshotDirectory_IsParsed()
    {
        var opts = ShowcaseCliOptions.Parse(new[] { "--screenshot", "captures" });

        Assert.True(opts.ScreenshotMode);
        Assert.Equal("captures", opts.ScreenshotDir);
    }

    [Fact]
    public void CliParser_RejectsLegacyAndFailoverConflict()
    {
        var ex = Assert.Throws<ArgumentException>(
            () => ShowcaseCliOptions.Parse(new[] { "--legacy-csg", "--allow-legacy-failover" }));

        Assert.Contains("--legacy-csg", ex.Message, StringComparison.Ordinal);
        Assert.Contains("--allow-legacy-failover", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void CliParser_RejectsLegacyAndStrictOnlyConflict()
    {
        var ex = Assert.Throws<ArgumentException>(
            () => ShowcaseCliOptions.Parse(new[] { "--legacy-csg", "--strict-robust-only" }));

        Assert.Contains("--legacy-csg", ex.Message, StringComparison.Ordinal);
        Assert.Contains("--strict-robust-only", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void CliParser_RejectsUnknownArgument()
    {
        var ex = Assert.Throws<ArgumentException>(
            () => ShowcaseCliOptions.Parse(new[] { "--not-a-real-option" }));

        Assert.Contains("--not-a-real-option", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Step3_StrictNoFailover_SucceedsWithRobustResult()
    {
        ShowcaseRuntimeOptions.UseLegacyCsg = false;
        ShowcaseRuntimeOptions.AllowLegacyFailover = false;

        (Solid step2, Solid cylY) = BuildStep2Input();

        Solid step3 = ShowcaseCsg.Difference(step2, cylY);
        Assert.True(step3.Mesh.Faces.Count > 0);
    }

    [Fact]
    public void Step3_StrictRobustOnly_SucceedsWithRobustResult()
    {
        ShowcaseRuntimeOptions.UseLegacyCsg = false;
        ShowcaseRuntimeOptions.AllowLegacyFailover = false;
        ShowcaseRuntimeOptions.UseClosureAttemptRobust = false;

        (Solid step2, Solid cylY) = BuildStep2Input();

        Solid step3 = ShowcaseCsg.Difference(step2, cylY);
        Assert.True(step3.Mesh.Faces.Count > 0);
    }

    [Fact]
    public void Step3_ExplicitFailover_SucceedsDirectlyWithoutFailover()
    {
        ShowcaseRuntimeOptions.UseLegacyCsg = false;
        ShowcaseRuntimeOptions.AllowLegacyFailover = true;

        using var output = new StringWriter();
        TextWriter previous = Console.Out;
        Console.SetOut(output);
        try
        {
            (Solid step2, Solid cylY) = BuildStep2Input();
            Solid step3 = ShowcaseCsg.Difference(step2, cylY);

            Assert.True(step3.Mesh.Faces.Count > 0);
            // No failover message since robust now succeeds directly
            Assert.DoesNotContain("using explicit legacy failover", output.ToString(), StringComparison.Ordinal);
        }
        finally
        {
            Console.SetOut(previous);
        }
    }

    [Fact]
    public void IssueSummary_IsDeterministicAndDeduplicated()
    {
        var issues = new[]
        {
            new RobustIssue(RobustIssueSeverity.Warning, RobustIssueCode.OutputMeshHasDegenerateFaces, "warn"),
            new RobustIssue(RobustIssueSeverity.Error, RobustIssueCode.OutputMeshNotEdgeManifold, "err-b"),
            new RobustIssue(RobustIssueSeverity.Info, RobustIssueCode.StageInvariantViolation, "info"),
            new RobustIssue(RobustIssueSeverity.Error, RobustIssueCode.OutputMeshNotClosed, "err-a"),
            new RobustIssue(RobustIssueSeverity.Error, RobustIssueCode.OutputMeshNotClosed, "err-a")
        };

        string summary = ShowcaseCsg.BuildIssueSummary(issues);
        Assert.Equal(
            "Error:OutputMeshNotClosed:err-a | Error:OutputMeshNotEdgeManifold:err-b | Warning:OutputMeshHasDegenerateFaces:warn | Info:StageInvariantViolation:info",
            summary);
    }

    [Fact]
    public void Step3_LegacyMode_BypassesRobustFailurePath()
    {
        ShowcaseRuntimeOptions.UseLegacyCsg = true;
        ShowcaseRuntimeOptions.AllowLegacyFailover = false;

        (Solid step2, Solid cylY) = BuildStep2Input();
        Solid step3 = ShowcaseCsg.Difference(step2, cylY);
        Assert.True(step3.Mesh.Faces.Count > 0);
    }

    private static (Solid Step2, Solid CylY) BuildStep2Input()
    {
        var y = new Vec3(0, 1, 0);
        var sphere = Primitives.Sphere(Vec3.Zero, 1.3, 3);
        var box = Primitives.Cube(Vec3.Zero, 1.8);
        var cylX = Primitives.Cylinder(new Vec3(-1.5, 0, 0), new Vec3(1, 0, 0), 0.5, 3.0);
        var cylY = Primitives.Cylinder(new Vec3(0, -1.5, 0), y, 0.5, 3.0);

        var step1 = ShowcaseCsg.Intersect(sphere, box);
        var step2 = ShowcaseCsg.Difference(step1, cylX);
        return (step2, cylY);
    }

    private static void ResetRuntimeOptions()
    {
        ShowcaseRuntimeOptions.UseLegacyCsg = false;
        ShowcaseRuntimeOptions.AllowLegacyFailover = false;
        ShowcaseRuntimeOptions.UseClosureAttemptRobust = true;
    }
}
