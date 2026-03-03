using MdCsg.Api;
using MdCsg.Math;

namespace MdCsg.Robust.Conformance;

public class RobustPerformanceBudgetTests
{
    private static readonly RobustOperationOptions StrictOpts = new()
    {
        Mode = RobustMode.Strict,
        Deterministic = true,
        UseRobustTriangulationKernel = true
    };

    [Theory]
    [InlineData("union")]
    [InlineData("intersection")]
    [InlineData("difference")]
    public void StableCorpus_OperationElapsed_StaysWithinBudget(string op)
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Cube(new Vec3(0.75, 0, 0), 2.0);

        var result = op switch
        {
            "union" => RobustCsg.Union(a, b, StrictOpts),
            "intersection" => RobustCsg.Intersect(a, b, StrictOpts),
            "difference" => RobustCsg.Difference(a, b, StrictOpts),
            _ => throw new ArgumentOutOfRangeException(nameof(op), op, null)
        };

        Assert.True(result.Succeeded, $"{op} should succeed in stable budget corpus.");
        Assert.True(
            result.Diagnostics.OperationElapsed <= TimeSpan.FromSeconds(10),
            $"{op} exceeded stable budget: {result.Diagnostics.OperationElapsed} > 00:00:10.");
    }

    [Fact]
    public void KnownBlocker_Step3_StaysWithinFailClosedBudget()
    {
        var y = new Vec3(0, 1, 0);
        var sphere = Primitives.Sphere(Vec3.Zero, 1.3, 3);
        var box = Primitives.Cube(Vec3.Zero, 1.8);
        var cylX = Primitives.Cylinder(new Vec3(-1.5, 0, 0), new Vec3(1, 0, 0), 0.5, 3.0);
        var cylY = Primitives.Cylinder(new Vec3(0, -1.5, 0), y, 0.5, 3.0);

        var step1 = RobustCsg.Intersect(sphere, box, StrictOpts);
        Assert.True(step1.Succeeded);
        var step2 = RobustCsg.Difference(new Solid(step1.Result!.Mesh), cylX, StrictOpts);
        Assert.True(step2.Succeeded);
        var step3 = RobustCsg.Difference(new Solid(step2.Result!.Mesh), cylY, StrictOpts);

        Assert.True(step3.Succeeded);
        Assert.True(
            step3.Diagnostics.OperationElapsed <= TimeSpan.FromMinutes(8),
            $"step3 budget exceeded: {step3.Diagnostics.OperationElapsed} > 00:08:00.");
    }
}
