using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG manifold invariants — Euler characteristic, closedness, orientation across all operations</summary>
public class CsgManifoldInvariantsPropertyTests
{
    [Fact]
    public void Union_CubeCube_Euler2()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var result = Csg.Union(a, b);
        Assert.Equal(2, MeshValidator.EulerCharacteristic(result.Mesh));
    }

    [Fact]
    public void Intersect_CubeCube_Euler2()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var result = Csg.Intersect(a, b);
        Assert.Equal(2, MeshValidator.EulerCharacteristic(result.Mesh));
    }

    [Fact]
    public void Difference_CubeCube_Euler2()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var result = Csg.Difference(a, b);
        Assert.Equal(2, MeshValidator.EulerCharacteristic(result.Mesh));
    }

    [Fact]
    public void Union_CubeSphere_ProducesFaces()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 2);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
        Assert.True(MeshValidator.HasValidFaceCycles(result.Mesh));
    }

    [Fact]
    public void Intersect_CubeSphere_ProducesFaces()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 2);
        var result = Csg.Intersect(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Difference_CubeSphere_ProducesFaces()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 3.0);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 0.5, 2);
        var result = Csg.Difference(a, b);
        Assert.True(result.FaceCount > a.Mesh.Faces.Count);
    }

    [Fact]
    public void Union_SphereSphere_ProducesFaces()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 2);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Difference_SphereSphere_ProducesFaces()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 2);
        var result = Csg.Difference(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Union_AllEdgesLinked()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var result = Csg.Union(a, b);
        Assert.True(MeshValidator.AllEdgesHaveTwins(result.Mesh));
    }

    [Fact]
    public void Intersect_AllEdgesLinked()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var result = Csg.Intersect(a, b);
        Assert.True(MeshValidator.AllEdgesHaveTwins(result.Mesh));
    }

    [Fact]
    public void Difference_AllEdgesLinked()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var result = Csg.Difference(a, b);
        Assert.True(MeshValidator.AllEdgesHaveTwins(result.Mesh));
    }

    [Fact]
    public void Union_ConsistentOrientation()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var result = Csg.Union(a, b);
        Assert.True(MeshValidator.IsConsistentlyOriented(result.Mesh));
    }

    [Fact]
    public void Intersect_ConsistentOrientation()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var result = Csg.Intersect(a, b);
        Assert.True(MeshValidator.IsConsistentlyOriented(result.Mesh));
    }

    [Fact]
    public void Difference_ConsistentOrientation()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var result = Csg.Difference(a, b);
        Assert.True(MeshValidator.IsConsistentlyOriented(result.Mesh));
    }

    [Fact]
    public void Union_ValidFaceCycles()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 1);
        var result = Csg.Union(a, b);
        Assert.True(MeshValidator.HasValidFaceCycles(result.Mesh));
    }
}
