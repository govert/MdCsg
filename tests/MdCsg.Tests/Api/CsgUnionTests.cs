using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Api;

/// <summary>Batch 27: CSG Union operation tests (20 tests)</summary>
public class CsgUnionTests
{
    [Fact]
    public void Union_OverlappingCubes_ProducesMesh()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Union_DisjointCubes_KeepsBothMeshes()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);
        var result = Csg.Union(a, b);
        Assert.Equal(24, result.FaceCount); // 12 + 12
    }

    [Fact]
    public void Union_DisjointCubes_NoIntersections()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);
        var result = Csg.Union(a, b);
        Assert.Equal(0, result.IntersectionSegmentCount);
    }

    [Fact]
    public void Union_OverlappingCubes_MoreFacesThanOne()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 12);
    }

    [Fact]
    public void Union_WithCustomOptions()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var opts = new CsgOptions { GridSize = 1e-6 };
        var result = Csg.Union(a, b, opts);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Union_WithWindingNumber()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var opts = new CsgOptions { UseWindingNumber = true };
        var result = Csg.Union(a, b, opts);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Union_CubeSphere_ProducesMesh()
    {
        var cube = new Solid(MeshFactory.CreateCube().Mesh);
        var sphere = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.8, 1).Mesh);
        var result = Csg.Union(cube, sphere);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Union_Result_HasVertices()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.VertexCount > 0);
    }

    [Fact]
    public void Union_Result_HasPatches()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.PatchCountA >= 1);
        Assert.True(result.PatchCountB >= 1);
    }

    [Fact]
    public void Union_DisjointCubes_TwoPatchesPerSolid()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);
        var result = Csg.Union(a, b);
        Assert.Equal(1, result.PatchCountA);
        Assert.Equal(1, result.PatchCountB);
    }

    [Fact]
    public void Union_YOffset_ProducesMesh()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0, 0.3, 0.3)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 12);
    }

    [Fact]
    public void Union_ZOffset_ProducesMesh()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0, 0.3)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 12);
    }

    [Fact]
    public void Union_SmallOverlap_ProducesMesh()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.9, 0.1, 0.1)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 12);
    }

    [Fact]
    public void Union_LargeOverlap_ProducesMesh()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.1, 0.1, 0.1)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 12);
    }

    [Fact]
    public void Union_SphereSphere_ProducesMesh()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1, 1).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1, 1).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Union_TetrahedronCube_ProducesMesh()
    {
        var cube = new Solid(MeshFactory.CreateCube().Mesh);
        var tet = new Solid(MeshFactory.CreateTetrahedron(new Vec3(0.5, 0.5, 0.5), 0.8).Mesh);
        var result = Csg.Union(cube, tet);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Union_NullOptions_UsesDefaults()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var result = Csg.Union(a, b, null);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Union_Commutative_SameFaceCount()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var ab = Csg.Union(a, b);
        var ba = Csg.Union(b, a);
        Assert.Equal(ab.FaceCount, ba.FaceCount);
    }

    [Fact]
    public void Union_DisjointSpheres_KeepsBoth()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 0.5, 1).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(5, 0, 0), 0.5, 1).Mesh);
        var result = Csg.Union(a, b);
        Assert.Equal(a.Mesh.Faces.Count + b.Mesh.Faces.Count, result.FaceCount);
    }

    [Fact]
    public void Union_Result_NoDegeneratePatches()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var result = Csg.Union(a, b);
        Assert.Equal(0, result.DegenerateCount);
    }
}
