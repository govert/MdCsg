using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Robust.Conformance;

public class RobustConformanceBacklogTests
{
    [Fact]
    public void CoplanarSharedFace_StrictMode_FailsFastWithCoplanarIssue()
    {
        var a = Primitives.Cube(Vec3.Zero, 2.0);
        var b = Primitives.Cube(new Vec3(2, 0, 0), 2.0); // shared face at x=1
        var result = RobustCsg.Union(a, b, new RobustOperationOptions
        {
            Mode = RobustMode.Strict,
            AnalyzeInputIntersection = true,
            TreatCoplanarIntersectionAsError = true,
            FailOnValidationError = true
        });

        Assert.False(result.Succeeded);
        Assert.Null(result.Result);
        Assert.Contains(result.Issues, i => i.Code == RobustIssueCode.InputIntersectionContainsCoplanarPairs);
        Assert.True(result.Diagnostics.ArrangementCoplanarFaceCountA > 0);
        Assert.True(result.Diagnostics.ArrangementCoplanarFaceCountB > 0);
    }

    [Fact]
    public void ThinSlab_HalfSpaceChain_StrictMode_StaysClosed()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var top = HalfSpace.FromPointAndNormal(new Vec3(0, 0, 0.01), Vec3.UnitZ);
        var bottom = HalfSpace.FromPointAndNormal(new Vec3(0, 0, -0.01), -Vec3.UnitZ);

        var r1 = new Solid(Csg.Intersect(cube, top).Mesh);
        var r2 = Csg.Intersect(r1, bottom);
        var robust = RobustCsg.Union(new Solid(r2.Mesh), r1, new RobustOperationOptions
        {
            Mode = RobustMode.Strict,
            AnalyzeInputIntersection = true,
            TreatOpenArrangementAsError = true,
            FailOnValidationError = true
        });

        Assert.True(robust.Succeeded);
        Assert.NotNull(robust.Result);
        Assert.Equal(0, MeshValidator.CountBoundaryEdges(robust.Result!.Mesh));
        Assert.DoesNotContain(robust.Issues, i => i.Severity == RobustIssueSeverity.Error);
    }

    [Fact(Skip = "Target behavior for the new robust kernel; boundary-only contacts must avoid spurious fragments.")]
    public void KissingContact_Intersection_ProducesNoSpuriousVolume()
    {
        var a = Primitives.Sphere(Vec3.Zero, 1.0, 3);
        var b = Primitives.Sphere(new Vec3(2.0, 0, 0), 1.0, 3); // tangent at one point
        var result = RobustCsg.Intersect(a, b, new RobustOperationOptions { Mode = RobustMode.Strict });

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Result);
        Assert.True(new Solid(result.Result!.Mesh).Volume() < 1e-6);
    }

    [Fact(Skip = "Target behavior for the new robust kernel; thin slabs currently trigger severe artifacts.")]
    public void ThinSlab_HalfSpaceChain_RemainsClosed()
    {
        var cube = Primitives.Cube(Vec3.Zero, 2.0);
        var top = HalfSpace.FromPointAndNormal(new Vec3(0, 0, 0.01), Vec3.UnitZ);
        var bottom = HalfSpace.FromPointAndNormal(new Vec3(0, 0, -0.01), -Vec3.UnitZ);

        var r1 = new Solid(Csg.Intersect(cube, top).Mesh);
        var r2 = Csg.Intersect(r1, bottom);
        var robust = RobustCsg.Union(new Solid(r2.Mesh), r1, new RobustOperationOptions { Mode = RobustMode.Strict });

        Assert.True(robust.Succeeded);
        Assert.NotNull(robust.Result);
        Assert.Equal(0, MeshValidator.CountBoundaryEdges(robust.Result!.Mesh));
    }
}
