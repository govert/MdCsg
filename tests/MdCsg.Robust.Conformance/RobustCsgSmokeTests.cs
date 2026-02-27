using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Robust;

namespace MdCsg.Robust.Conformance;

public class RobustCsgSmokeTests
{
    [Fact]
    public void Union_Smoke_Succeeds()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Cube(new Vec3(0.75, 0, 0), 2.0);

        var result = RobustCsg.Union(a, b);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Result);
        Assert.True(result.Result!.FaceCount > 0);
        Assert.True(result.Diagnostics.TotalElapsed >= TimeSpan.Zero);
        Assert.True(result.Diagnostics.OperationElapsed >= TimeSpan.Zero);
    }

    [Fact]
    public void Union_IsDeterministicInStrictMode_ForBasicCase()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Cube(new Vec3(0.5, 0.5, 0), 2.0);
        var opts = new RobustOperationOptions
        {
            Mode = RobustMode.Strict,
            Deterministic = true
        };

        var r1 = RobustCsg.Union(a, b, opts);
        var r2 = RobustCsg.Union(a, b, opts);

        Assert.True(r1.Succeeded);
        Assert.True(r2.Succeeded);
        Assert.NotNull(r1.Result);
        Assert.NotNull(r2.Result);
        Assert.Equal(r1.Result!.FaceCount, r2.Result!.FaceCount);
        Assert.Equal(r1.Result.VertexCount, r2.Result.VertexCount);
    }

    [Fact]
    public void StrictMode_RejectsOpenInputMesh()
    {
        var open = Solid.FromTriangles(
        [
            new Triangle3(
                new Vec3(0, 0, 0),
                new Vec3(1, 0, 0),
                new Vec3(0, 1, 0))
        ]);
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var opts = new RobustOperationOptions
        {
            Mode = RobustMode.Strict,
            ValidateInput = true,
            FailOnValidationError = true
        };

        var result = RobustCsg.Union(open, cube, opts);

        Assert.False(result.Succeeded);
        Assert.Null(result.Result);
        Assert.Contains(result.Issues, i => i.Code == RobustIssueCode.InputMeshNotClosed);
    }
}
