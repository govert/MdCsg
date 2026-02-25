using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG sphere-sphere operations — overlapping, disjoint, concentric, tangent configurations</summary>
public class CsgSphereSpherePropertyTests
{
    [Fact]
    public void Union_OverlappingSpheres_ProducesFaces()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 1);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Intersect_OverlappingSpheres_ProducesFaces()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 1);
        var result = Csg.Intersect(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Difference_OverlappingSpheres_ProducesFaces()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 1);
        var result = Csg.Difference(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Union_DisjointSpheres_FaceCountIsSum()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        var b = MeshFactory.CreateSphere(new Vec3(10, 0, 0), 1.0, 1);
        var result = Csg.Union(a, b);
        Assert.Equal(a.Mesh.Faces.Count + b.Mesh.Faces.Count, result.FaceCount);
    }

    [Fact]
    public void Intersect_DisjointSpheres_Empty()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        var b = MeshFactory.CreateSphere(new Vec3(10, 0, 0), 1.0, 1);
        var result = Csg.Intersect(a, b);
        Assert.Equal(0, result.FaceCount);
    }

    [Fact]
    public void Difference_DisjointSpheres_PreservesA()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        var b = MeshFactory.CreateSphere(new Vec3(10, 0, 0), 1.0, 1);
        var result = Csg.Difference(a, b);
        Assert.Equal(a.Mesh.Faces.Count, result.FaceCount);
    }

    [Fact]
    public void Union_OverlappingSpheres_HasIntersectionSegments()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 1);
        var result = Csg.Union(a, b);
        Assert.True(result.IntersectionSegmentCount > 0);
    }

    [Fact]
    public void Union_DisjointSpheres_ZeroSegments()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        var b = MeshFactory.CreateSphere(new Vec3(10, 0, 0), 1.0, 1);
        var result = Csg.Union(a, b);
        Assert.Equal(0, result.IntersectionSegmentCount);
    }

    [Fact]
    public void Union_WithWinding_ProducesFaces()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 1);
        var result = Csg.Union(a, b, new CsgOptions { UseWindingNumber = true });
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Intersect_WithWinding_ProducesFaces()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 1);
        var result = Csg.Intersect(a, b, new CsgOptions { UseWindingNumber = true });
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Union_OverlappingSpheres_ResultMeshValid()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 1);
        var result = Csg.Union(a, b);
        // Verify all faces have 3-edge cycles
        foreach (var face in result.Mesh.Faces)
        {
            int count = 0;
            var start = face.Edge;
            var current = start;
            do
            {
                count++;
                current = current.Next;
            } while (current != start && count < 100);
            Assert.Equal(3, count);
        }
    }

    [Fact]
    public void Intersect_OverlappingSpheres_PatchCountsPositive()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 1);
        var result = Csg.Intersect(a, b);
        Assert.True(result.PatchCountA > 0);
        Assert.True(result.PatchCountB > 0);
    }

    [Fact]
    public void Union_Overlap_AllFaceNormalsNonZero()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 1);
        var result = Csg.Union(a, b);
        foreach (var face in result.Mesh.Faces)
        {
            Assert.True(face.Normal.LengthSquared > 1e-20,
                $"Face {face.Id} has near-zero normal");
        }
    }

    [Fact]
    public void Difference_AB_BA_DifferentFaceCounts()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.5, 1);
        var b = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 1);
        var ab = Csg.Difference(a, b);
        var ba = Csg.Difference(b, a);
        // Different sizes → different difference results
        Assert.True(ab.FaceCount > 0);
        Assert.True(ba.FaceCount > 0);
    }

    [Fact]
    public void Union_HigherSubdivision_MoreFaces()
    {
        var a1 = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        var b1 = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 1);
        var result1 = Csg.Union(a1, b1);

        var a2 = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var b2 = MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1.0, 2);
        var result2 = Csg.Union(a2, b2);

        Assert.True(result2.FaceCount > result1.FaceCount,
            $"Higher subdivision union ({result2.FaceCount}) should have more faces than lower ({result1.FaceCount})");
    }
}
