using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG nested operations — (A-B)-C, A∪B∪C, multi-step chains, volume tracking</summary>
public class CsgNestedOperationTests
{
    private static double Volume(CsgResult r) => VolumeCalculator.ComputeAbsoluteVolume(r.Mesh);
    private static double Volume(Solid s) => VolumeCalculator.ComputeAbsoluteVolume(s.Mesh);

    [Fact]
    public void DoubleDifference_VolumeDecreases()
    {
        // (A - B) - C: each subtraction reduces volume
        var a = new Solid(MeshFactory.CreateCube(Vec3.Zero, 2).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.2, 0.2, 0.2), 0.5).Mesh);
        var c = new Solid(MeshFactory.CreateCube(new Vec3(1.0, 0.2, 0.2), 0.5).Mesh);

        var ab = Csg.Difference(a, b);
        double volAB = Volume(ab);
        var abSolid = new Solid(ab.Mesh);
        var abc = Csg.Difference(abSolid, c);
        double volABC = Volume(abc);

        double volA = Volume(a);
        Assert.True(volAB < volA + 0.01);
        Assert.True(volABC < volAB + 0.01);
        Assert.True(volABC > 0);
    }

    [Fact]
    public void UnionChain_MonotonicallyGrowing()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.8, 0, 0)).Mesh);
        var c = new Solid(MeshFactory.CreateCube(new Vec3(1.6, 0, 0)).Mesh);

        var ab = Csg.Union(a, b);
        double volAB = Volume(ab);

        var abSolid = new Solid(ab.Mesh);
        var abc = Csg.Union(abSolid, c);
        double volABC = Volume(abc);

        Assert.True(volABC >= volAB - 0.01);
    }

    [Fact]
    public void IntersectionChain_MonotonicallyDecreasing()
    {
        var a = new Solid(MeshFactory.CreateCube(Vec3.Zero, 2).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5), 2).Mesh);
        var c = new Solid(MeshFactory.CreateCube(new Vec3(1.0, 1.0, 1.0), 2).Mesh);

        var ab = Csg.Intersect(a, b);
        double volAB = Volume(ab);

        var abSolid = new Solid(ab.Mesh);
        var abc = Csg.Intersect(abSolid, c);
        double volABC = Volume(abc);

        Assert.True(volABC <= volAB + 0.1);
        Assert.True(volABC >= 0);
    }

    [Fact]
    public void DiffThenUnion_RestoresVolume()
    {
        // (A - B) ∪ B ≈ A ∪ B
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);

        var diff = Csg.Difference(a, b);
        var diffSolid = new Solid(diff.Mesh);
        var restored = Csg.Union(diffSolid, b);
        double volRestored = Volume(restored);
        double volUnion = Volume(Csg.Union(a, b));

        Assert.True(System.Math.Abs(volRestored - volUnion) < 0.1,
            $"(A-B)∪B={volRestored:F4} vs A∪B={volUnion:F4}");
    }

    [Fact]
    public void MultipleDisjointUnions_VolumeAdds()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(3, 0, 0)).Mesh);
        var c = new Solid(MeshFactory.CreateCube(new Vec3(6, 0, 0)).Mesh);

        var ab = Csg.Union(a, b);
        var abSolid = new Solid(ab.Mesh);
        var abc = Csg.Union(abSolid, c);

        double vol = Volume(abc);
        Assert.True(System.Math.Abs(vol - 3.0) < 0.01, $"Expected 3.0, got {vol:F4}");
    }

    [Fact]
    public void DisjointDifference_NoEffect()
    {
        // A - B where B is far away → result ≈ A
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(10, 0, 0)).Mesh);

        var result = Csg.Difference(a, b);
        double volA = Volume(a);
        double volResult = Volume(result);

        Assert.True(System.Math.Abs(volResult - volA) < 0.01);
    }

    [Fact]
    public void DisjointIntersection_EmptyResult()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(10, 0, 0)).Mesh);

        var result = Csg.Intersect(a, b);
        Assert.Equal(0, result.FaceCount);
    }

    [Fact]
    public void SubtractTwice_SameShape_Idempotent()
    {
        // (A - B) - B should be the same as A - B
        var a = new Solid(MeshFactory.CreateCube(Vec3.Zero, 2).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5), 0.5).Mesh);

        var ab = Csg.Difference(a, b);
        double volAB = Volume(ab);

        var abSolid = new Solid(ab.Mesh);
        var abb = Csg.Difference(abSolid, b);
        double volABB = Volume(abb);

        Assert.True(System.Math.Abs(volAB - volABB) < 0.5,
            $"(A-B)={volAB:F4} vs (A-B)-B={volABB:F4}");
    }

    [Fact]
    public void UnionThenIntersect_Bounded()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
        var c = new Solid(MeshFactory.CreateCube(new Vec3(0.25, 0.25, 0.25)).Mesh);

        var ab = Csg.Union(a, b);
        var abSolid = new Solid(ab.Mesh);
        var result = Csg.Intersect(abSolid, c);

        double vol = Volume(result);
        Assert.True(vol > 0);
        Assert.True(vol <= Volume(c) + 0.2,
            $"Intersection volume {vol:F4} should be <= C volume {Volume(c):F4}");
    }

    [Fact]
    public void IntersectThenDifference()
    {
        var a = new Solid(MeshFactory.CreateCube(Vec3.Zero, 2).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5), 2).Mesh);
        var c = new Solid(MeshFactory.CreateCube(new Vec3(1, 1, 1), 0.5).Mesh);

        var ab = Csg.Intersect(a, b);
        var abSolid = new Solid(ab.Mesh);
        var result = Csg.Difference(abSolid, c);

        double vol = Volume(result);
        Assert.True(vol >= 0);
        Assert.True(vol <= Volume(ab) + 0.1);
    }
}
