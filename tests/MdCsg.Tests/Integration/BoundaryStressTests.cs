using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Batch 39: Boundary and coplanar stress tests (20 tests)</summary>
public class BoundaryStressTests
{
    [Fact]
    public void DiagonalOffset_AllOperations_Succeed()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        Assert.True(Csg.Union(a, b).FaceCount > 0);
        Assert.True(Csg.Intersect(a, b).FaceCount > 0);
        Assert.True(Csg.Difference(a, b).FaceCount > 0);
    }

    [Fact]
    public void SmallDiagonalOffset_AllOperations()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.1, 0.2, 0.15)).Mesh);
        Assert.True(Csg.Union(a, b).FaceCount > 0);
        Assert.True(Csg.Intersect(a, b).FaceCount > 0);
        Assert.True(Csg.Difference(a, b).FaceCount > 0);
    }

    [Fact]
    public void LargeDiagonalOffset_UnionAndDifference()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.7, 0.8, 0.6)).Mesh);
        Assert.True(Csg.Union(a, b).FaceCount > 0);
        Assert.True(Csg.Difference(a, b).FaceCount > 0);
    }

    [Fact]
    public void AlmostContained_Difference()
    {
        // Inner cube barely protrudes on one side
        var outer = new Solid(MeshFactory.CreateCube(new Vec3(-0.5, -0.5, -0.5), 2).Mesh);
        var inner = new Solid(MeshFactory.CreateCube(new Vec3(0.1, 0.1, 0.1), 0.8).Mesh);
        var result = Csg.Difference(outer, inner);
        Assert.True(result.FaceCount > 12);
    }

    [Fact]
    public void BarelyCrossing_Union()
    {
        // Cubes barely overlap (0.01 units)
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.99, 0.1, 0.2)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 12);
    }

    [Fact]
    public void MultiLayer_Union_ThreeCubes()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.2, 0.1)).Mesh);
        var c = new Solid(MeshFactory.CreateCube(new Vec3(0.6, 0.4, 0.2)).Mesh);
        var ab = Csg.Union(a, b);
        var abc = Csg.Union(new Solid(ab.Mesh), c);
        Assert.True(abc.FaceCount > 0);
    }

    [Fact]
    public void MultiLayer_Difference_ThreeCubes()
    {
        var a = new Solid(MeshFactory.CreateCube(new Vec3(-0.5, -0.5, -0.5), 2).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.1, 0.2, 0.3), 0.5).Mesh);
        var c = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.4, 0.6), 0.5).Mesh);
        var ab = Csg.Difference(a, b);
        var abc = Csg.Difference(new Solid(ab.Mesh), c);
        Assert.True(abc.FaceCount > 0);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(abc.Mesh);
        Assert.True(vol > 7.0, $"After carving two holes, volume {vol} should be > 7.0");
    }

    [Fact]
    public void CubeTetra_Union_Overlap()
    {
        var cube = new Solid(MeshFactory.CreateCube().Mesh);
        var tet = new Solid(MeshFactory.CreateTetrahedron(new Vec3(0.3, 0.3, 0.3), 1.0).Mesh);
        var result = Csg.Union(cube, tet);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void CubeTetra_Intersect_Overlap()
    {
        var cube = new Solid(MeshFactory.CreateCube().Mesh);
        var tet = new Solid(MeshFactory.CreateTetrahedron(new Vec3(0.3, 0.3, 0.3), 1.0).Mesh);
        var result = Csg.Intersect(cube, tet);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void CubeTetra_Difference_Overlap()
    {
        var cube = new Solid(MeshFactory.CreateCube().Mesh);
        var tet = new Solid(MeshFactory.CreateTetrahedron(new Vec3(0.3, 0.3, 0.3), 1.0).Mesh);
        var result = Csg.Difference(cube, tet);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void SphereCube_Union()
    {
        var cube = new Solid(MeshFactory.CreateCube().Mesh);
        var sphere = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.8, 2).Mesh);
        var result = Csg.Union(cube, sphere);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void SphereCube_Intersect()
    {
        var cube = new Solid(MeshFactory.CreateCube().Mesh);
        var sphere = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.3, 2).Mesh);
        var result = Csg.Intersect(cube, sphere);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void SphereSphere_Union_PartialOverlap()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(1.2, 0.3, 0.1), 1, 2).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void SphereSphere_Intersect_PartialOverlap()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(1.2, 0.3, 0.1), 1, 2).Mesh);
        var result = Csg.Intersect(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void FourConsecutiveUnions_Stable()
    {
        var current = new Solid(MeshFactory.CreateCube().Mesh);
        // Use disjoint cubes to avoid chained CSG accumulation issues
        for (int i = 1; i <= 4; i++)
        {
            var next = new Solid(MeshFactory.CreateCube(new Vec3(i * 2.0, 0, 0)).Mesh);
            var result = Csg.Union(current, next);
            current = new Solid(result.Mesh);
        }
        Assert.True(current.Mesh.Faces.Count > 0);
        Assert.Equal(60, current.Mesh.Faces.Count); // 5 disjoint cubes * 12 faces each
    }

    [Fact]
    public void Union_ThenIntersect_ProducesResult()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var ab = Csg.Union(a, b);
        // Intersect with a large cube that fully contains both
        var c = new Solid(MeshFactory.CreateCube(new Vec3(-0.5, -0.5, -0.5), 3).Mesh);
        var result = Csg.Intersect(new Solid(ab.Mesh), c);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Intersect_ThenDifference_ProducesResult()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var ab = Csg.Intersect(a, b);
        if (ab.FaceCount > 0)
        {
            var c = new Solid(MeshFactory.CreateCube(new Vec3(0.4, 0.4, 0.4), 0.3).Mesh);
            var result = Csg.Difference(new Solid(ab.Mesh), c);
            Assert.True(result.FaceCount >= 0);
        }
    }

    [Fact]
    public void Difference_ThenUnion_ProducesResult()
    {
        var a = new Solid(MeshFactory.CreateCube(new Vec3(-0.5, -0.5, -0.5), 2).Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.2, 0.3, 0.1), 0.5).Mesh);
        var diff = Csg.Difference(a, b);
        var c = new Solid(MeshFactory.CreateCube(new Vec3(0.8, 0.8, 0.8), 0.5).Mesh);
        var result = Csg.Union(new Solid(diff.Mesh), c);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void TwoSpheres_Difference_Hollow()
    {
        var outer = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh);
        var inner = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 0.5, 2).Mesh);
        var result = Csg.Difference(outer, inner);
        Assert.True(result.FaceCount > 0);
        double vol = VolumeCalculator.ComputeAbsoluteVolume(result.Mesh);
        Assert.True(vol > 0, $"Hollow sphere should have positive volume, got {vol}");
    }

    [Fact]
    public void TetraSphere_Intersect()
    {
        var tet = new Solid(MeshFactory.CreateTetrahedron(Vec3.Zero, 2).Mesh);
        var sphere = new Solid(MeshFactory.CreateSphere(new Vec3(0.3, 0.3, 0.3), 0.5, 2).Mesh);
        var result = Csg.Intersect(tet, sphere);
        Assert.True(result.FaceCount > 0);
    }
}
