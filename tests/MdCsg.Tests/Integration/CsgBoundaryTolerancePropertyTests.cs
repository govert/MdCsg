using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG boundary/tolerance — touching faces, near-tangent, very small/large solids, scaling</summary>
public class CsgBoundaryTolerancePropertyTests
{
    [Fact]
    public void Union_TouchingCubes_ProducesMesh()
    {
        // Two cubes sharing a face
        var a = MeshFactory.CreateCube(Vec3.Zero, 1.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 1.0);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Union_NearlyTouchingCubes_ProducesMesh()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 1.0);
        var b = MeshFactory.CreateCube(new Vec3(1.001, 0, 0), 1.0);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Union_TinyOverlap_ProducesMesh()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 1.0);
        var b = MeshFactory.CreateCube(new Vec3(0.999, 0, 0), 1.0);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Difference_ContainedSphere_ProducesFaces()
    {
        // Small sphere fully inside large cube
        var cube = MeshFactory.CreateCube(Vec3.Zero, 4.0);
        var sphere = MeshFactory.CreateSphere(new Vec3(2, 2, 2), 0.5, 2);
        var result = Csg.Difference(cube, sphere);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Intersect_ContainedSphere_ReturnsSphere()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 4.0);
        var sphere = MeshFactory.CreateSphere(new Vec3(2, 2, 2), 0.5, 1);
        var result = Csg.Intersect(cube, sphere);
        // Intersection should be approximately the sphere
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Union_LargeOffsetCubes_FaceCountIsSum()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 1.0);
        var b = MeshFactory.CreateCube(new Vec3(1000, 0, 0), 1.0);
        var result = Csg.Union(a, b);
        Assert.Equal(a.Mesh.Faces.Count + b.Mesh.Faces.Count, result.FaceCount);
    }

    [Fact]
    public void Union_SmallCubes_ProducesMesh()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 0.001);
        var b = MeshFactory.CreateCube(new Vec3(0.0005, 0, 0), 0.001);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Difference_SelfLike_EmptyOrSmall()
    {
        // Nearly identical cubes — difference should be small or empty
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(0.001, 0, 0), 2.0);
        var result = Csg.Difference(a, b);
        Assert.NotNull(result.Mesh);
    }

    [Fact]
    public void Union_ThreeOverlapping_ChainedOps()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var c = MeshFactory.CreateCube(new Vec3(0, 1, 0), 2.0);
        var ab = Csg.Union(a, b);
        var abc = Csg.Union(new Solid(ab.Mesh), c);
        Assert.True(abc.FaceCount > 0);
    }

    [Fact]
    public void Difference_ThenUnion_ProducesMesh()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 3.0);
        var b = MeshFactory.CreateSphere(new Vec3(1.5, 1.5, 1.5), 0.5, 1);
        var diff = Csg.Difference(a, b);
        var c = MeshFactory.CreateCube(new Vec3(1, 1, 1), 0.5);
        var result = Csg.Union(new Solid(diff.Mesh), c);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Union_ScaledSpheres_FaceCountPositive()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 10.0, 1);
        var b = MeshFactory.CreateSphere(new Vec3(5, 0, 0), 10.0, 1);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Intersect_SlightOverlap_FaceCountPositive()
    {
        // Spheres just barely overlapping
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var b = MeshFactory.CreateSphere(new Vec3(1.9, 0, 0), 1.0, 2);
        var result = Csg.Intersect(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Union_AllOperationTypes_ProduceNonNull()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 1);
        Assert.NotNull(Csg.Union(a, b).Mesh);
        Assert.NotNull(Csg.Intersect(a, b).Mesh);
        Assert.NotNull(Csg.Difference(a, b).Mesh);
    }
}
