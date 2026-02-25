using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG stress with higher subdivisions — more faces, consistent results, valid output</summary>
public class CsgHigherSubdivisionPropertyTests
{
    [Fact]
    public void Union_Sub2Spheres_ProducesFaces()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 2);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Intersect_Sub2Spheres_ProducesFaces()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 2);
        var result = Csg.Intersect(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Difference_Sub2Spheres_ProducesFaces()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 2);
        var result = Csg.Difference(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Union_Sub2_MoreFacesThanSub1()
    {
        var a1 = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        var b1 = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 1);
        var r1 = Csg.Union(a1, b1);

        var a2 = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var b2 = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 2);
        var r2 = Csg.Union(a2, b2);

        Assert.True(r2.FaceCount > r1.FaceCount);
    }

    [Fact]
    public void Union_Sub2Spheres_AllFaceNormalsNonZero()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 2);
        var result = Csg.Union(a, b);
        foreach (var face in result.Mesh.Faces)
        {
            Assert.True(face.Normal.LengthSquared > 1e-20);
        }
    }

    [Fact]
    public void Union_Sub2Spheres_AllFacesAreTriangles()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 2);
        var result = Csg.Union(a, b);
        foreach (var face in result.Mesh.Faces)
        {
            int count = 0;
            var start = face.Edge;
            var current = start;
            do { count++; current = current.Next; } while (current != start && count < 100);
            Assert.Equal(3, count);
        }
    }

    [Fact]
    public void Intersect_Sub2_DisjointSpheres_Empty()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var b = MeshFactory.CreateSphere(new Vec3(10, 0, 0), 1.0, 2);
        var result = Csg.Intersect(a, b);
        Assert.Equal(0, result.FaceCount);
    }

    [Fact]
    public void Union_Sub2_DisjointSpheres_FaceCountIsSum()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var b = MeshFactory.CreateSphere(new Vec3(10, 0, 0), 1.0, 2);
        var result = Csg.Union(a, b);
        Assert.Equal(a.Mesh.Faces.Count + b.Mesh.Faces.Count, result.FaceCount);
    }

    [Fact]
    public void Union_Sub2_IntersectionSegmentCount_Positive()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 2);
        var result = Csg.Union(a, b);
        Assert.True(result.IntersectionSegmentCount > 0);
    }

    [Fact]
    public void Union_Sub2_PatchCounts_Positive()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 2);
        var result = Csg.Union(a, b);
        Assert.True(result.PatchCountA > 0);
        Assert.True(result.PatchCountB > 0);
    }
}
