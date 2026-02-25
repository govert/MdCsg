using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG Union — volume, geometry, and metadata properties</summary>
public class CsgUnionVolumePropertyTests
{
    [Fact]
    public void Union_OverlappingCubes_HasFaces()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5));
        var result = Csg.Union(a, b);
        Assert.True(result.Mesh.Faces.Count > 0);
    }

    [Fact]
    public void Union_DisjointCubes_HasFaces()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(5, 5, 5));
        var result = Csg.Union(a, b);
        Assert.True(result.Mesh.Faces.Count > 0);
    }

    [Fact]
    public void Union_DisjointCubes_FaceCountIsSum()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(5, 5, 5));
        var result = Csg.Union(a, b);
        Assert.Equal(a.Mesh.Faces.Count + b.Mesh.Faces.Count, result.Mesh.Faces.Count);
    }

    [Fact]
    public void Union_DisjointCubes_BoundsEncloseBoth()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(5, 5, 5));
        var result = Csg.Union(a, b);
        var bounds = result.Mesh.GetBounds();
        Assert.True(bounds.Contains(new Vec3(0.5, 0.5, 0.5)), "Should contain A center");
        Assert.True(bounds.Contains(new Vec3(5.5, 5.5, 5.5)), "Should contain B center");
    }

    [Fact]
    public void Union_IsCommutative_FaceCount()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3));
        var ab = Csg.Union(a, b);
        var ba = Csg.Union(
            MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)),
            MeshFactory.CreateCube());
        Assert.Equal(ab.Mesh.Faces.Count, ba.Mesh.Faces.Count);
    }

    [Fact]
    public void Union_OverlappingCubes_MoreFacesThanOne()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5));
        var result = Csg.Union(a, b);
        Assert.True(result.Mesh.Faces.Count > a.Mesh.Faces.Count,
            "Union of overlapping cubes should have more faces than a single cube");
    }

    [Fact]
    public void Union_HasIntersectionSegments_WhenOverlapping()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5));
        var result = Csg.Union(a, b);
        Assert.True(result.IntersectionSegmentCount > 0);
    }

    [Fact]
    public void Union_DisjointCubes_NoIntersectionSegments()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(10, 10, 10));
        var result = Csg.Union(a, b);
        Assert.Equal(0, result.IntersectionSegmentCount);
    }

    [Fact]
    public void Union_CubeSphere_HasFaces()
    {
        var cube = MeshFactory.CreateCube();
        var sphere = MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 1.0), 0.5, 1);
        var result = Csg.Union(cube, sphere);
        Assert.True(result.Mesh.Faces.Count > 0);
    }

    [Fact]
    public void Union_PatchCountsPositive_WhenOverlapping()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3));
        var result = Csg.Union(a, b);
        Assert.True(result.PatchCountA > 0);
        Assert.True(result.PatchCountB > 0);
    }

    [Fact]
    public void Union_FacesHaveValidCycles()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3));
        var result = Csg.Union(a, b);
        foreach (var face in result.Mesh.Faces)
        {
            var start = face.Edge;
            var current = start;
            int count = 0;
            do { count++; current = current.Next; } while (current != start && count < 10);
            Assert.Equal(3, count);
        }
    }

    [Fact]
    public void Union_SphereSphere_HasFaces()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        var b = MeshFactory.CreateSphere(new Vec3(1.0, 0, 0), 1.0, 1);
        var result = Csg.Union(a, b);
        Assert.True(result.Mesh.Faces.Count > 0);
    }

    [Fact]
    public void Union_WithWindingNumber_ProducesFaces()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3));
        var options = new CsgOptions { UseWindingNumber = true };
        var result = Csg.Union(a, b, options);
        Assert.True(result.Mesh.Faces.Count > 0);
    }

    [Fact]
    public void Union_BoundsContainBothInputCenters()
    {
        // Disjoint union should contain both centers
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(5, 5, 5));
        var result = Csg.Union(a, b);
        var bounds = result.Mesh.GetBounds();
        Assert.True(bounds.Contains(a.Bounds.Center), "Union bounds should contain A center");
        Assert.True(bounds.Contains(b.Bounds.Center), "Union bounds should contain B center");
    }
}
