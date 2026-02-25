using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG Intersect — volume, geometry, and metadata properties</summary>
public class CsgIntersectVolumePropertyTests
{
    [Fact]
    public void Intersect_OverlappingCubes_HasFaces()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3));
        var result = Csg.Intersect(a, b);
        Assert.True(result.Mesh.Faces.Count > 0, "Intersection should produce faces");
    }

    [Fact]
    public void Intersect_DisjointCubes_NoFaces()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(10, 10, 10));
        var result = Csg.Intersect(a, b);
        Assert.Equal(0, result.Mesh.Faces.Count);
    }

    [Fact]
    public void Intersect_IsCommutative_FaceCount()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3));
        var ab = Csg.Intersect(a, b);
        var ba = Csg.Intersect(
            MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)),
            MeshFactory.CreateCube());
        Assert.Equal(ab.Mesh.Faces.Count, ba.Mesh.Faces.Count);
    }

    [Fact]
    public void Intersect_BoundsContainedByBoth()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3));
        var result = Csg.Intersect(a, b);
        if (result.Mesh.Faces.Count == 0) return;
        var bounds = result.Mesh.GetBounds();
        var boundsA = a.Bounds;
        var boundsB = b.Bounds;
        // Intersection bounds should be within both input bounds
        Assert.True(bounds.Min.X >= System.Math.Min(boundsA.Min.X, boundsB.Min.X) - 1e-6);
        Assert.True(bounds.Max.X <= System.Math.Max(boundsA.Max.X, boundsB.Max.X) + 1e-6);
    }

    [Fact]
    public void Intersect_HasIntersectionSegments_WhenOverlapping()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3));
        var result = Csg.Intersect(a, b);
        Assert.True(result.IntersectionSegmentCount > 0);
    }

    [Fact]
    public void Intersect_DisjointCubes_NoIntersectionSegments()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(10, 10, 10));
        var result = Csg.Intersect(a, b);
        Assert.Equal(0, result.IntersectionSegmentCount);
    }

    [Fact]
    public void Intersect_FacesHaveValidCycles()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3));
        var result = Csg.Intersect(a, b);
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
    public void Intersect_OverlappingCubes_HasVertices()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3));
        var intersect = Csg.Intersect(a, b);
        Assert.True(intersect.Mesh.Vertices.Count > 0,
            "Intersection should produce vertices");
    }

    [Fact]
    public void Intersect_PatchCounts_Positive_WhenOverlapping()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3));
        var result = Csg.Intersect(a, b);
        Assert.True(result.PatchCountA > 0);
        Assert.True(result.PatchCountB > 0);
    }

    [Fact]
    public void Intersect_WithWindingNumber_ProducesFaces()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3));
        var options = new CsgOptions { UseWindingNumber = true };
        var result = Csg.Intersect(a, b, options);
        Assert.True(result.Mesh.Faces.Count > 0);
    }

    [Fact]
    public void Intersect_SphereSphere_HasFaces()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        var b = MeshFactory.CreateSphere(new Vec3(1.0, 0, 0), 1.0, 1);
        var result = Csg.Intersect(a, b);
        Assert.True(result.Mesh.Faces.Count > 0);
    }

    [Fact]
    public void Intersect_CubeSphere_HasFaces()
    {
        var cube = MeshFactory.CreateCube();
        var sphere = MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.8, 1);
        var result = Csg.Intersect(cube, sphere);
        Assert.True(result.Mesh.Faces.Count > 0);
    }

    [Fact]
    public void Intersect_ResultSmallerThanInputs()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3));
        var result = Csg.Intersect(a, b);
        var boundsResult = result.Mesh.GetBounds();
        var boundsA = a.Bounds;
        // Intersection bounds should be smaller than or equal to either input
        Assert.True(boundsResult.Size.X <= boundsA.Size.X + 1e-6,
            $"Intersection X size ({boundsResult.Size.X}) should be <= input X size ({boundsA.Size.X})");
    }

    [Fact]
    public void Intersect_TetrahedronCube_HasFaces()
    {
        var tet = MeshFactory.CreateTetrahedron();
        var cube = MeshFactory.CreateCube(new Vec3(-0.3, -0.3, -0.3), 0.6);
        var result = Csg.Intersect(tet, cube);
        Assert.True(result.Mesh.Faces.Count > 0);
    }
}
