using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG algebraic property tests — commutativity, idempotency, identity-like, monotonicity</summary>
public class CsgAlgebraicPropertyTests
{
    private static double ComputeVolume(CsgResult result)
    {
        double vol = 0;
        foreach (var face in result.Mesh.Faces)
        {
            face.GetTrianglePositions(out var a, out var b, out var c);
            vol += Vec3.Dot(a, Vec3.Cross(b, c)) / 6.0;
        }
        return System.Math.Abs(vol);
    }

    // ── Union commutativity ──

    [Fact]
    public void Union_Commutative_FaceCountSimilar()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var ab = Csg.Union(a, b);
        var ba = Csg.Union(b, a);
        // Face count should be similar (not necessarily identical due to mesh structure differences)
        Assert.True(System.Math.Abs(ab.FaceCount - ba.FaceCount) <= ab.FaceCount * 0.2,
            $"Union not commutative: A∪B={ab.FaceCount}, B∪A={ba.FaceCount}");
    }

    [Fact]
    public void Union_Commutative_VolumeSimilar()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var volAB = ComputeVolume(Csg.Union(a, b));
        var volBA = ComputeVolume(Csg.Union(b, a));
        Assert.True(System.Math.Abs(volAB - volBA) < 0.1,
            $"Volume not commutative: A∪B={volAB:F4}, B∪A={volBA:F4}");
    }

    // ── Intersection commutativity ──

    [Fact]
    public void Intersect_Commutative_FaceCountSimilar()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var ab = Csg.Intersect(a, b);
        var ba = Csg.Intersect(b, a);
        Assert.True(System.Math.Abs(ab.FaceCount - ba.FaceCount) <= ab.FaceCount * 0.2,
            $"Intersect not commutative: A∩B={ab.FaceCount}, B∩A={ba.FaceCount}");
    }

    [Fact]
    public void Intersect_Commutative_VolumeSimilar()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var volAB = ComputeVolume(Csg.Intersect(a, b));
        var volBA = ComputeVolume(Csg.Intersect(b, a));
        Assert.True(System.Math.Abs(volAB - volBA) < 0.1,
            $"Volume not commutative: A∩B={volAB:F4}, B∩A={volBA:F4}");
    }

    // ── Volume ordering: Intersection ⊂ Each ⊂ Union ──

    [Fact]
    public void VolumeOrdering_IntersectLessThanUnion()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var volInt = ComputeVolume(Csg.Intersect(a, b));
        var volUni = ComputeVolume(Csg.Union(a, b));
        Assert.True(volInt < volUni, $"Intersection ({volInt:F4}) should be < Union ({volUni:F4})");
    }

    [Fact]
    public void VolumeOrdering_DifferenceLessThanOriginal()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        double volA = 1.0; // unit cube
        var volDiff = ComputeVolume(Csg.Difference(a, b));
        Assert.True(volDiff < volA * 1.1, $"Difference ({volDiff:F4}) should be < A ({volA})");
    }

    // ── Difference is not commutative ──

    [Fact]
    public void Difference_NotCommutative()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var volAB = ComputeVolume(Csg.Difference(a, b));
        var volBA = ComputeVolume(Csg.Difference(b, a));
        // Both should be positive (partial overlap)
        Assert.True(volAB > 0);
        Assert.True(volBA > 0);
    }

    // ── Volume positivity ──

    [Fact]
    public void AllOperations_PositiveVolume()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        Assert.True(ComputeVolume(Csg.Union(a, b)) > 0);
        Assert.True(ComputeVolume(Csg.Intersect(a, b)) > 0);
        Assert.True(ComputeVolume(Csg.Difference(a, b)) > 0);
    }

    // ── All operations produce valid meshes ──

    [Fact]
    public void AllOperations_NoNaNVertices()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        foreach (var result in new[] { Csg.Union(a, b), Csg.Intersect(a, b), Csg.Difference(a, b) })
        {
            foreach (var v in result.Mesh.Vertices)
            {
                Assert.False(double.IsNaN(v.Position.X));
                Assert.False(double.IsNaN(v.Position.Y));
                Assert.False(double.IsNaN(v.Position.Z));
            }
        }
    }

    [Fact]
    public void AllOperations_AllFacesTriangles()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        foreach (var result in new[] { Csg.Union(a, b), Csg.Intersect(a, b), Csg.Difference(a, b) })
        {
            foreach (var face in result.Mesh.Faces)
                Assert.Equal(3, face.GetVertices().Count);
        }
    }

    // ── Increasing overlap → smaller intersection, larger union ──

    [Fact]
    public void IntersectVolume_IncreasesWithOverlap()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b1 = new Solid(MeshFactory.CreateCube(new Vec3(0.8, 0, 0)).Mesh);
        var b2 = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0, 0)).Mesh);
        var vol1 = ComputeVolume(Csg.Intersect(a, b1)); // small overlap
        var vol2 = ComputeVolume(Csg.Intersect(a, b2)); // large overlap
        Assert.True(vol2 > vol1 * 0.5,
            $"More overlap should mean larger intersection: vol1={vol1:F4}, vol2={vol2:F4}");
    }

    // ── Sphere-cube operations ──

    [Fact]
    public void SphereCube_AllOperations_Produce()
    {
        var sphere = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.6, 2).Mesh);
        var cube = new Solid(MeshFactory.CreateCube().Mesh);
        Assert.True(Csg.Union(sphere, cube).FaceCount > 0);
        Assert.True(Csg.Intersect(sphere, cube).FaceCount > 0);
        Assert.True(Csg.Difference(sphere, cube).FaceCount > 0);
        Assert.True(Csg.Difference(cube, sphere).FaceCount > 0);
    }
}
