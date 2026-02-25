using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG winding number vs raycast — both methods should produce consistent results</summary>
public class CsgWindingRaycastConsistencyPropertyTests
{
    [Fact]
    public void Union_CubeSphere_BothMethodsProduceFaces()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 1);
        var ray = Csg.Union(a, b);
        var wind = Csg.Union(a, b, new CsgOptions { UseWindingNumber = true });
        Assert.True(ray.FaceCount > 0);
        Assert.True(wind.FaceCount > 0);
    }

    [Fact]
    public void Intersect_CubeSphere_BothMethodsProduceFaces()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 1);
        var ray = Csg.Intersect(a, b);
        var wind = Csg.Intersect(a, b, new CsgOptions { UseWindingNumber = true });
        Assert.True(ray.FaceCount > 0);
        Assert.True(wind.FaceCount > 0);
    }

    [Fact]
    public void Difference_CubeSphere_BothMethodsProduceFaces()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 1);
        var ray = Csg.Difference(a, b);
        var wind = Csg.Difference(a, b, new CsgOptions { UseWindingNumber = true });
        Assert.True(ray.FaceCount > 0);
        Assert.True(wind.FaceCount > 0);
    }

    [Fact]
    public void Union_SphereSphere_SameSegmentCount()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 1);
        var ray = Csg.Union(a, b);
        var wind = Csg.Union(a, b, new CsgOptions { UseWindingNumber = true });
        // Same intersection graph → same segment count
        Assert.Equal(ray.IntersectionSegmentCount, wind.IntersectionSegmentCount);
    }

    [Fact]
    public void Union_DisjointCubes_BothMethodsReturnSum()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 1.0);
        var b = MeshFactory.CreateCube(new Vec3(10, 0, 0), 1.0);
        var ray = Csg.Union(a, b);
        var wind = Csg.Union(a, b, new CsgOptions { UseWindingNumber = true });
        Assert.Equal(ray.FaceCount, wind.FaceCount);
    }

    [Fact]
    public void Intersect_DisjointCubes_BothMethodsReturnEmpty()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 1.0);
        var b = MeshFactory.CreateCube(new Vec3(10, 0, 0), 1.0);
        var ray = Csg.Intersect(a, b);
        var wind = Csg.Intersect(a, b, new CsgOptions { UseWindingNumber = true });
        Assert.Equal(0, ray.FaceCount);
        Assert.Equal(0, wind.FaceCount);
    }

    [Fact]
    public void Union_SphereSphere_SamePatchCounts()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 1);
        var ray = Csg.Union(a, b);
        var wind = Csg.Union(a, b, new CsgOptions { UseWindingNumber = true });
        Assert.Equal(ray.PatchCountA, wind.PatchCountA);
        Assert.Equal(ray.PatchCountB, wind.PatchCountB);
    }

    [Fact]
    public void Intersect_SphereSphere_FaceCounts_BothPositive()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 1);
        var ray = Csg.Intersect(a, b);
        var wind = Csg.Intersect(a, b, new CsgOptions { UseWindingNumber = true });
        Assert.True(ray.FaceCount > 0);
        Assert.True(wind.FaceCount > 0);
    }

    [Fact]
    public void Difference_SphereSphere_BothMethodsProduceFaces()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 1);
        var ray = Csg.Difference(a, b);
        var wind = Csg.Difference(a, b, new CsgOptions { UseWindingNumber = true });
        Assert.True(ray.FaceCount > 0);
        Assert.True(wind.FaceCount > 0);
    }

    [Fact]
    public void Union_CubeCube_BothMethodsProduceFaces()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var ray = Csg.Union(a, b);
        var wind = Csg.Union(a, b, new CsgOptions { UseWindingNumber = true });
        Assert.True(ray.FaceCount > 0);
        Assert.True(wind.FaceCount > 0);
    }

    [Fact]
    public void Difference_CubeCube_BothMethodsProduceFaces()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var ray = Csg.Difference(a, b);
        var wind = Csg.Difference(a, b, new CsgOptions { UseWindingNumber = true });
        Assert.True(ray.FaceCount > 0);
        Assert.True(wind.FaceCount > 0);
    }
}
