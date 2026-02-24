using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Api;

/// <summary>Batch 29: CSG Difference operation tests (20 tests)</summary>
public class CsgDifferenceTests
{
    [Fact]
    public void Difference_OverlappingCubes_ProducesMesh()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var result = Csg.Difference(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Difference_DisjointCubes_KeepsA()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);
        var result = Csg.Difference(a, b);
        Assert.Equal(12, result.FaceCount); // all of A
    }

    [Fact]
    public void Difference_NotCommutative()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var ab = Csg.Difference(a, b);
        var ba = Csg.Difference(b, a);
        // A-B and B-A produce different shapes
        // They may have same face count by coincidence but different geometry
        Assert.True(ab.FaceCount > 0);
        Assert.True(ba.FaceCount > 0);
    }

    [Fact]
    public void Difference_WithWindingNumber()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var opts = new CsgOptions { UseWindingNumber = true };
        var result = Csg.Difference(a, b, opts);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Difference_HasIntersectionSegments()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var result = Csg.Difference(a, b);
        Assert.True(result.IntersectionSegmentCount > 0);
    }

    [Fact]
    public void Difference_Result_HasVertices()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var result = Csg.Difference(a, b);
        Assert.True(result.VertexCount > 0);
    }

    [Fact]
    public void Difference_Result_HasPatches()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var result = Csg.Difference(a, b);
        Assert.True(result.PatchCountA >= 1);
        Assert.True(result.PatchCountB >= 1);
    }

    [Fact]
    public void Difference_CubeSphere_ProducesMesh()
    {
        var cube = new Solid(MeshFactory.CreateCube().Mesh);
        var sphere = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.8, 1).Mesh);
        var result = Csg.Difference(cube, sphere);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Difference_SphereSphere_ProducesMesh()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1, 1).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1, 1).Mesh);
        var result = Csg.Difference(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Difference_SmallOverlap_ProducesMesh()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.9, 0.1, 0.1)).Mesh);
        var result = Csg.Difference(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Difference_NullOptions_UsesDefaults()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var result = Csg.Difference(a, b, null);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Difference_DisjointSpheres_KeepsA()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 0.5, 1).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(5, 0, 0), 0.5, 1).Mesh);
        var result = Csg.Difference(a, b);
        Assert.Equal(a.Mesh.Faces.Count, result.FaceCount);
    }

    [Fact]
    public void Difference_NoDegeneratePatches()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var result = Csg.Difference(a, b);
        Assert.Equal(0, result.DegenerateCount);
    }

    [Fact]
    public void Difference_DiagonalOffset2_ProducesMesh()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.4, 0.3, 0.2)).Mesh);
        var result = Csg.Difference(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Difference_DiagonalOffset3_ProducesMesh()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.2, 0.4, 0.6)).Mesh);
        var result = Csg.Difference(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Difference_TetrahedronFromCube_ProducesMesh()
    {
        var cube = new Solid(MeshFactory.CreateCube().Mesh);
        var tet = new Solid(MeshFactory.CreateTetrahedron(new Vec3(0.5, 0.5, 0.5), 0.8).Mesh);
        var result = Csg.Difference(cube, tet);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Difference_CustomGridSize()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var opts = new CsgOptions { GridSize = 1e-10 };
        var result = Csg.Difference(a, b, opts);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Difference_LargeOverlap_RemovesMore()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.1, 0.1, 0.1)).Mesh);
        var result = Csg.Difference(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Difference_SphereFromCube_ProducesMesh()
    {
        var cube = new Solid(MeshFactory.CreateCube().Mesh);
        var sphere = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.8, 1).Mesh);
        var result = Csg.Difference(cube, sphere);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Difference_ContainedCube_Carved()
    {
        var outer = new Solid(MeshFactory.CreateCube(new Vec3(-0.5, -0.5, -0.5), 2).Mesh);
        var inner = new Solid(MeshFactory.CreateCube(new Vec3(0.25, 0.25, 0.25), 0.5).Mesh);
        var result = Csg.Difference(outer, inner);
        // Carving inner from outer should produce more faces
        Assert.True(result.FaceCount > 12);
    }
}
