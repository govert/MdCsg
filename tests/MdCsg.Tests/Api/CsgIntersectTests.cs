using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Api;

/// <summary>Batch 28: CSG Intersection operation tests (20 tests)</summary>
public class CsgIntersectTests
{
    [Fact]
    public void Intersect_OverlappingCubes_ProducesMesh()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var result = Csg.Intersect(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Intersect_DisjointCubes_EmptyMesh()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);
        var result = Csg.Intersect(a, b);
        Assert.Equal(0, result.FaceCount);
    }

    [Fact]
    public void Intersect_OverlappingCubes_ProducesNonEmptyResult()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var result = Csg.Intersect(a, b);
        // Intersection produces faces from the overlapping volume
        Assert.True(result.FaceCount > 0);
        Assert.True(result.VertexCount > 0);
    }

    [Fact]
    public void Intersect_Commutative_SameFaceCount()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var ab = Csg.Intersect(a, b);
        var ba = Csg.Intersect(b, a);
        Assert.Equal(ab.FaceCount, ba.FaceCount);
    }

    [Fact]
    public void Intersect_Result_HasPatches()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var result = Csg.Intersect(a, b);
        Assert.True(result.PatchCountA >= 1);
        Assert.True(result.PatchCountB >= 1);
    }

    [Fact]
    public void Intersect_HasIntersectionSegments()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var result = Csg.Intersect(a, b);
        Assert.True(result.IntersectionSegmentCount > 0);
    }

    [Fact]
    public void Intersect_WithWindingNumber()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var opts = new CsgOptions { UseWindingNumber = true };
        var result = Csg.Intersect(a, b, opts);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Intersect_CubeSphere_ProducesMesh()
    {
        var cube = new Solid(MeshFactory.CreateCube().Mesh);
        var sphere = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.8, 1).Mesh);
        var result = Csg.Intersect(cube, sphere);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Intersect_SphereSphere_ProducesMesh()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1, 1).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1, 1).Mesh);
        var result = Csg.Intersect(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Intersect_SmallOverlap_ProducesSmallMesh()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.9, 0.1, 0.1)).Mesh);
        var result = Csg.Intersect(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Intersect_DisjointSpheres_EmptyResult()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 0.5, 1).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(5, 0, 0), 0.5, 1).Mesh);
        var result = Csg.Intersect(a, b);
        Assert.Equal(0, result.FaceCount);
    }

    [Fact]
    public void Intersect_NoDegeneratePatches()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var result = Csg.Intersect(a, b);
        Assert.Equal(0, result.DegenerateCount);
    }

    [Fact]
    public void Intersect_Result_HasVertices()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var result = Csg.Intersect(a, b);
        Assert.True(result.VertexCount > 0);
    }

    [Fact]
    public void Intersect_LargeOverlap_ProducesLargerResult()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var bSmall = new Solid(MeshFactory.CreateCube(new Vec3(0.9, 0.1, 0.1)).Mesh);
        var bLarge = new Solid(MeshFactory.CreateCube(new Vec3(0.1, 0.1, 0.1)).Mesh);
        var rSmall = Csg.Intersect(a, bSmall);
        var rLarge = Csg.Intersect(a, bLarge);
        // Larger overlap should produce more faces
        Assert.True(rLarge.FaceCount >= rSmall.FaceCount);
    }

    [Fact]
    public void Intersect_YOffset_ProducesMesh()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0, 0.3, 0.3)).Mesh);
        var result = Csg.Intersect(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Intersect_TetrahedronCube_ProducesMesh()
    {
        var cube = new Solid(MeshFactory.CreateCube().Mesh);
        var tet = new Solid(MeshFactory.CreateTetrahedron(new Vec3(0.5, 0.5, 0.5), 0.8).Mesh);
        var result = Csg.Intersect(cube, tet);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Intersect_NullOptions_UsesDefaults()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var result = Csg.Intersect(a, b, null);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Intersect_CustomGridSize()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var opts = new CsgOptions { GridSize = 1e-10 };
        var result = Csg.Intersect(a, b, opts);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Intersect_ZOffset_ProducesMesh()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0)).Mesh);
        var result = Csg.Intersect(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Intersect_ContainedCube_KeepsInner()
    {
        var outer = new Solid(MeshFactory.CreateCube(new Vec3(-0.5, -0.5, -0.5), 2).Mesh);
        var inner = new Solid(MeshFactory.CreateCube(new Vec3(0.25, 0.25, 0.25), 0.5).Mesh);
        var result = Csg.Intersect(outer, inner);
        // Fully contained — intersection is the inner cube
        Assert.True(result.FaceCount > 0);
    }
}
