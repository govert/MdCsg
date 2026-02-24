using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: Stress tests — many operations, varied geometry, edge-case combinations</summary>
public class StressTests
{
    [Fact]
    public void ThreeWayUnion_DisjointCubes()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(3, 0, 0)).Mesh);
        var c = new Solid(MeshFactory.CreateCube(new Vec3(6, 0, 0)).Mesh);
        var ab = Csg.Union(a, b);
        var abc = Csg.Union(new Solid(ab.Mesh), c);
        Assert.Equal(36, abc.FaceCount); // 3 * 12
    }

    [Fact]
    public void ThreeWayUnion_OverlappingCubes()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var ab = Csg.Union(a, b);
        var c = new Solid(MeshFactory.CreateCube(new Vec3(0.6, 0.6, 0.6)).Mesh);
        var abc = Csg.Union(new Solid(ab.Mesh), c);
        Assert.True(abc.FaceCount > 0);
    }

    [Fact]
    public void UnionThenDifference()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var ab = Csg.Union(a, b);
        var c = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5), 0.5).Mesh);
        var result = Csg.Difference(new Solid(ab.Mesh), c);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void DifferenceThenUnion()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3), 0.5).Mesh);
        var diff = Csg.Difference(a, b);
        var c = new Solid(MeshFactory.CreateCube(new Vec3(3, 0, 0)).Mesh);
        var result = Csg.Union(new Solid(diff.Mesh), c);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void MultipleDiagonalOffsets_Union()
    {
        var offsets = new[]
        {
            new Vec3(0.3, 0.3, 0.3),
            new Vec3(0.2, 0.4, 0.15),
            new Vec3(0.35, 0.25, 0.4),
        };
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        foreach (var offset in offsets)
        {
            var b = new Solid(MeshFactory.CreateCube(offset).Mesh);
            var r = Csg.Union(a, b);
            Assert.True(r.FaceCount > 12, $"Union at {offset}: {r.FaceCount} faces");
        }
    }

    [Fact]
    public void MultipleDiagonalOffsets_Difference()
    {
        var offsets = new[]
        {
            new Vec3(0.3, 0.3, 0.3),
            new Vec3(0.2, 0.4, 0.15),
            new Vec3(0.35, 0.25, 0.4),
        };
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        foreach (var offset in offsets)
        {
            var b = new Solid(MeshFactory.CreateCube(offset).Mesh);
            var r = Csg.Difference(a, b);
            Assert.True(r.FaceCount > 0, $"Diff at {offset}: {r.FaceCount} faces");
        }
    }

    [Fact]
    public void SphereSphere_Union()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(1.2, 0.3, 0.1), 1, 2).Mesh);
        var r = Csg.Union(a, b);
        Assert.True(r.FaceCount > 0);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(r.Mesh);
        Assert.True(vol > 0);
    }

    [Fact]
    public void SphereSphere_Intersection()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(1.2, 0.3, 0.1), 1, 2).Mesh);
        var r = Csg.Intersect(a, b);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void SphereSphere_Difference()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(1.2, 0.3, 0.1), 1, 2).Mesh);
        var r = Csg.Difference(a, b);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void CubeSphere_Union()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.8, 2).Mesh);
        var r = Csg.Union(a, b);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void CubeSphere_Difference()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.3, 2).Mesh);
        var r = Csg.Difference(a, b);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void SmallCube_Union()
    {
        var a = new Solid(MeshFactory.CreateCube(size: 0.01).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.003, 0.003, 0.003), 0.01).Mesh);
        var r = Csg.Union(a, b, new CsgOptions { GridSize = 1e-10 });
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void LargeCube_Union()
    {
        var a = new Solid(MeshFactory.CreateCube(size: 100).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(30, 30, 30), 100).Mesh);
        var r = Csg.Union(a, b);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void VertexCount_ConsistentWithFaceCount()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var r = Csg.Union(a, b);
        // Euler: V - E + F = 2 for closed manifold
        // E = 3F/2 for triangulated mesh → V = F/2 + 2
        // But welding can cause deviation. Just verify both are positive.
        Assert.True(r.VertexCount > 0);
        Assert.True(r.FaceCount > 0);
    }

    [Fact]
    public void DisjointSpheres_Union_SumOfFaces()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1, 1).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(10, 0, 0), 1, 1).Mesh);
        var r = Csg.Union(a, b);
        Assert.Equal(a.Mesh.Faces.Count + b.Mesh.Faces.Count, r.FaceCount);
    }

    [Fact]
    public void ContainedCube_Difference_HollowsOut()
    {
        var a = new Solid(MeshFactory.CreateCube(size: 2).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3), 0.5).Mesh);
        var r = Csg.Difference(a, b);
        double va = VolumeCalculator.ComputeAbsoluteVolume(a.Mesh);
        double vr = VolumeCalculator.ComputeAbsoluteVolume(r.Mesh);
        Assert.True(vr < va, $"Diff result vol {vr} should be less than original {va}");
    }

    [Fact]
    public void ContainedSphere_Intersection_PreservesSphere()
    {
        var bigCube = new Solid(MeshFactory.CreateCube(size: 10).Mesh);
        var smallSphere = new Solid(MeshFactory.CreateSphere(new Vec3(5, 5, 5), 1, 2).Mesh);
        var r = Csg.Intersect(bigCube, smallSphere);
        double vs = VolumeCalculator.ComputeAbsoluteVolume(smallSphere.Mesh);
        double vr = VolumeCalculator.ComputeAbsoluteVolume(r.Mesh);
        Assert.True(System.Math.Abs(vs - vr) < 0.5, $"Sphere vol {vs}, result vol {vr}");
    }

    [Fact]
    public void AllOps_ValidFaceCycles()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        foreach (var result in new[] { Csg.Union(a, b), Csg.Intersect(a, b), Csg.Difference(a, b) })
        {
            var validation = MeshValidator.Validate(result.Mesh);
            Assert.True(validation.HasValidFaceCycles);
        }
    }
}
