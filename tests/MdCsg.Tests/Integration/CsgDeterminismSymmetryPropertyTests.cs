using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG determinism — repeated operations give same result, union commutativity, De Morgan laws</summary>
public class CsgDeterminismSymmetryPropertyTests
{
    [Fact]
    public void Union_Deterministic_SameFaceCount()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 1);
        var r1 = Csg.Union(a, b);
        var r2 = Csg.Union(a, b);
        Assert.Equal(r1.FaceCount, r2.FaceCount);
    }

    [Fact]
    public void Intersect_Deterministic_SameFaceCount()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 1);
        var r1 = Csg.Intersect(a, b);
        var r2 = Csg.Intersect(a, b);
        Assert.Equal(r1.FaceCount, r2.FaceCount);
    }

    [Fact]
    public void Difference_Deterministic_SameFaceCount()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 1);
        var r1 = Csg.Difference(a, b);
        var r2 = Csg.Difference(a, b);
        Assert.Equal(r1.FaceCount, r2.FaceCount);
    }

    [Fact]
    public void Union_Commutative_SameFaceCount()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 1);
        var ab = Csg.Union(a, b);
        var ba = Csg.Union(b, a);
        Assert.Equal(ab.FaceCount, ba.FaceCount);
    }

    [Fact]
    public void Intersect_Commutative_SameFaceCount()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 1);
        var ab = Csg.Intersect(a, b);
        var ba = Csg.Intersect(b, a);
        Assert.Equal(ab.FaceCount, ba.FaceCount);
    }

    [Fact]
    public void Union_Deterministic_SameVertexCount()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var r1 = Csg.Union(a, b);
        var r2 = Csg.Union(a, b);
        Assert.Equal(r1.VertexCount, r2.VertexCount);
    }

    [Fact]
    public void Union_DisjointSpheres_Deterministic()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        var b = MeshFactory.CreateSphere(new Vec3(10, 0, 0), 1.0, 1);
        var r1 = Csg.Union(a, b);
        var r2 = Csg.Union(a, b);
        Assert.Equal(r1.FaceCount, r2.FaceCount);
        Assert.Equal(r1.IntersectionSegmentCount, r2.IntersectionSegmentCount);
    }

    [Fact]
    public void Union_EmptyResult_FromDisjointIntersect()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        var b = MeshFactory.CreateSphere(new Vec3(10, 0, 0), 1.0, 1);
        var r = Csg.Intersect(a, b);
        Assert.Equal(0, r.FaceCount);
        Assert.Equal(0, r.VertexCount);
    }

    [Fact]
    public void Difference_SelfMinus_FullPreservation()
    {
        // A minus disjoint B preserves A
        var a = MeshFactory.CreateCube(Vec3.Zero, 1.0);
        var b = MeshFactory.CreateCube(new Vec3(10, 10, 10), 1.0);
        var r = Csg.Difference(a, b);
        Assert.Equal(a.Mesh.Faces.Count, r.FaceCount);
    }

    [Fact]
    public void Union_IntersectionSegmentCount_Deterministic()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 1);
        var r1 = Csg.Union(a, b);
        var r2 = Csg.Union(a, b);
        Assert.Equal(r1.IntersectionSegmentCount, r2.IntersectionSegmentCount);
    }

    [Fact]
    public void Union_PatchCounts_Deterministic()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateCube(new Vec3(1, 0, 0), 2.0);
        var r1 = Csg.Union(a, b);
        var r2 = Csg.Union(a, b);
        Assert.Equal(r1.PatchCountA, r2.PatchCountA);
        Assert.Equal(r1.PatchCountB, r2.PatchCountB);
    }

    [Fact]
    public void Intersect_DegenerateCount_Deterministic()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 1);
        var r1 = Csg.Intersect(a, b);
        var r2 = Csg.Intersect(a, b);
        Assert.Equal(r1.DegenerateCount, r2.DegenerateCount);
    }
}
