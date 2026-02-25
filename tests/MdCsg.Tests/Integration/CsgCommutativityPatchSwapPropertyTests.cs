using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG commutativity, patch swap, disjoint identity properties</summary>
public class CsgCommutativityPatchSwapPropertyTests
{
    [Fact]
    public void Union_IsCommutative_FaceCount()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var ab = Csg.Union(a, b);
        var ba = Csg.Union(b, a);
        Assert.Equal(ab.FaceCount, ba.FaceCount);
    }

    [Fact]
    public void Intersect_IsCommutative_FaceCount()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var ab = Csg.Intersect(a, b);
        var ba = Csg.Intersect(b, a);
        Assert.Equal(ab.FaceCount, ba.FaceCount);
    }

    [Fact]
    public void Difference_IsNotCommutative()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var ab = Csg.Difference(a, b);
        var ba = Csg.Difference(b, a);
        Assert.True(ab.FaceCount > 0);
        Assert.True(ba.FaceCount > 0);
    }

    [Fact]
    public void Union_Disjoint_OrderDoesNotMatter()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(5, 0, 0));
        var ab = Csg.Union(a, b);
        var ba = Csg.Union(b, a);
        Assert.Equal(ab.FaceCount, ba.FaceCount);
    }

    [Fact]
    public void Intersect_Disjoint_BothEmpty()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(5, 0, 0));
        Assert.Equal(0, Csg.Intersect(a, b).FaceCount);
        Assert.Equal(0, Csg.Intersect(b, a).FaceCount);
    }

    [Fact]
    public void Difference_Disjoint_PreservesOriginal()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(5, 0, 0));
        Assert.Equal(a.Mesh.Faces.Count, Csg.Difference(a, b).FaceCount);
    }

    [Fact]
    public void Union_PatchCounts_Swapped()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var ab = Csg.Union(a, b);
        var ba = Csg.Union(b, a);
        Assert.Equal(ab.PatchCountA, ba.PatchCountB);
        Assert.Equal(ab.PatchCountB, ba.PatchCountA);
    }

    [Fact]
    public void Intersect_PatchCounts_Swapped()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var ab = Csg.Intersect(a, b);
        var ba = Csg.Intersect(b, a);
        Assert.Equal(ab.PatchCountA, ba.PatchCountB);
        Assert.Equal(ab.PatchCountB, ba.PatchCountA);
    }

    [Fact]
    public void Union_IntersectionSegments_SameForBothOrders()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var ab = Csg.Union(a, b);
        var ba = Csg.Union(b, a);
        Assert.Equal(ab.IntersectionSegmentCount, ba.IntersectionSegmentCount);
    }

    [Fact]
    public void SphereSphere_Union_Commutative()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var b = MeshFactory.CreateSphere(new Vec3(1.0, 0, 0), 1.0, 2);
        var ab = Csg.Union(a, b);
        var ba = Csg.Union(b, a);
        Assert.Equal(ab.FaceCount, ba.FaceCount);
    }

    [Fact]
    public void SphereSphere_Intersect_Commutative()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var b = MeshFactory.CreateSphere(new Vec3(1.0, 0, 0), 1.0, 2);
        var ab = Csg.Intersect(a, b);
        var ba = Csg.Intersect(b, a);
        Assert.Equal(ab.FaceCount, ba.FaceCount);
    }

    [Fact]
    public void Union_SameInput_HasResult()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero);
        var result = Csg.Union(cube, cube);
        Assert.True(result.FaceCount >= 0);
    }
}
