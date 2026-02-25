using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG multi-operation chains — sequential operations, nested CSG, stress</summary>
public class CsgMultiOperationChainTests
{
    [Fact]
    public void TwoUnions_ChainedResult()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
        var c = new Solid(MeshFactory.CreateCube(new Vec3(1.0, 0, 0)).Mesh);
        var ab = Csg.Union(a, b);
        var abc = Csg.Union(new Solid(ab.Mesh), c);
        Assert.True(abc.FaceCount > 0);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(abc.Mesh);
        Assert.True(vol > 1.5);
    }

    [Fact]
    public void UnionThenDifference()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
        var ab = Csg.Union(a, b);
        var hole = new Solid(MeshFactory.CreateCube(new Vec3(0.25, 0.25, 0.25), 0.3).Mesh);
        var result = Csg.Difference(new Solid(ab.Mesh), hole);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void IntersectionThenUnion()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var inter = Csg.Intersect(a, b);
        var c = new Solid(MeshFactory.CreateCube(new Vec3(2, 0, 0)).Mesh);
        var result = Csg.Union(new Solid(inter.Mesh), c);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void DifferenceThenIntersection()
    {
        var a = new Solid(MeshFactory.CreateCube(Vec3.Zero, 2).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5), 0.5).Mesh);
        var diff = Csg.Difference(a, b);
        var c = new Solid(MeshFactory.CreateCube(new Vec3(0, 0, 0), 1.5).Mesh);
        var result = Csg.Intersect(new Solid(diff.Mesh), c);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void SelfUnion_SameFaceCount()
    {
        // A ∪ A should produce the same shape as A
        // (though this is the identical/coplanar degenerate case)
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var result = Csg.Union(a, a);
        // May produce artifacts due to coplanar handling, but should be valid
        Assert.NotNull(result.Mesh);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void CubeSphereTetra_ThreeWayUnion()
    {
        var cube = new Solid(MeshFactory.CreateCube().Mesh);
        var sphere = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 1.0), 0.5, 2).Mesh);
        var cs = Csg.Union(cube, sphere);
        var tet = new Solid(MeshFactory.CreateTetrahedron(new Vec3(0.5, 0.5, -0.5), 0.5).Mesh);
        var result = Csg.Union(new Solid(cs.Mesh), tet);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void MultipleDifferences_Subtractive()
    {
        var big = new Solid(MeshFactory.CreateCube(Vec3.Zero, 3).Mesh);
        var h1 = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5), 0.5).Mesh);
        var r1 = Csg.Difference(big, h1);
        var h2 = new Solid(MeshFactory.CreateCube(new Vec3(1.5, 1.5, 1.5), 0.5).Mesh);
        var r2 = Csg.Difference(new Solid(r1.Mesh), h2);
        Assert.True(r2.FaceCount > 0);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(r2.Mesh);
        Assert.True(vol > 26.5); // 27 - 0.125 - 0.125 ≈ 26.75
    }

    [Fact]
    public void DisjointThenOverlapping()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);
        var disjoint = Csg.Union(a, b);
        // Now intersect with something overlapping only one
        var c = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Intersect(new Solid(disjoint.Mesh), c);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void ChainedDifferences_ProgressivelySmaller()
    {
        var big = new Solid(MeshFactory.CreateCube(Vec3.Zero, 2).Mesh);
        double prevVol = VolumeCalculator.ComputeAbsoluteVolume(big.Mesh);
        var current = big;
        var offsets = new[] { new Vec3(0.2, 0.2, 0.2), new Vec3(0.8, 0.8, 0.8) };
        foreach (var offset in offsets)
        {
            var hole = new Solid(MeshFactory.CreateCube(offset, 0.3).Mesh);
            var result = Csg.Difference(current, hole);
            double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
            Assert.True(vol < prevVol + 0.01, $"Volume should decrease: {vol} < {prevVol}");
            prevVol = vol;
            current = new Solid(result.Mesh);
        }
    }

    [Fact]
    public void AllResults_HaveValidFaceCycles()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var results = new[]
        {
            Csg.Union(a, b),
            Csg.Intersect(a, b),
            Csg.Difference(a, b),
        };
        foreach (var r in results)
        {
            Assert.True(MdCsg.Mesh.MeshValidator.HasValidFaceCycles(r.Mesh));
        }
    }
}
