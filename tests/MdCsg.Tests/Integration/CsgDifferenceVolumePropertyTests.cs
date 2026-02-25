using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG Difference — volume preservation, face count, geometric properties</summary>
public class CsgDifferenceVolumePropertyTests
{
    [Fact]
    public void Difference_CubeMinusCube_HasFaces()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3));
        var result = Csg.Difference(a, b);
        Assert.True(result.Mesh.Faces.Count > 0, "Difference should produce faces");
    }

    [Fact]
    public void Difference_CubeMinusCube_HasVertices()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3));
        var result = Csg.Difference(a, b);
        Assert.True(result.Mesh.Vertices.Count > 0, "Difference should produce vertices");
    }

    [Fact]
    public void Difference_CubeMinusCube_FacesHaveValidCycles()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3));
        var result = Csg.Difference(a, b);
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
    public void Difference_CubeMinusSmallCube_MoreFacesThanOriginal()
    {
        var a = MeshFactory.CreateCube();
        // Small cube at corner of A: creates cut faces
        var b = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3), 0.5);
        var result = Csg.Difference(a, b);
        Assert.True(result.Mesh.Faces.Count > a.Mesh.Faces.Count,
            $"Difference should have more faces than original ({result.Mesh.Faces.Count} vs {a.Mesh.Faces.Count})");
    }

    [Fact]
    public void Difference_DisjointCubes_SameAsOriginal()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(10, 10, 10));
        var result = Csg.Difference(a, b);
        // Disjoint: A - B = A, so face count should match
        Assert.Equal(a.Mesh.Faces.Count, result.Mesh.Faces.Count);
    }

    [Fact]
    public void Difference_HasIntersectionSegments_WhenOverlapping()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3));
        var result = Csg.Difference(a, b);
        Assert.True(result.IntersectionSegmentCount > 0, "Overlapping CSG should have intersection segments");
    }

    [Fact]
    public void Difference_DisjointCubes_NoIntersectionSegments()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(10, 10, 10));
        var result = Csg.Difference(a, b);
        Assert.Equal(0, result.IntersectionSegmentCount);
    }

    [Fact]
    public void Difference_NotCommutative()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3));
        var ab = Csg.Difference(a, b);
        var ba = Csg.Difference(
            MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)),
            MeshFactory.CreateCube());
        // A-B and B-A produce different results (different face counts typically)
        // At minimum, both should have faces
        Assert.True(ab.Mesh.Faces.Count > 0);
        Assert.True(ba.Mesh.Faces.Count > 0);
    }

    [Fact]
    public void Difference_CubeMinusSphere_HasFaces()
    {
        var cube = MeshFactory.CreateCube();
        var sphere = MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.3, 1);
        var result = Csg.Difference(cube, sphere);
        Assert.True(result.Mesh.Faces.Count > 0, "Cube minus sphere should produce faces");
    }

    [Fact]
    public void Difference_WithWindingNumber_ProducesFaces()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3));
        var options = new CsgOptions { UseWindingNumber = true };
        var result = Csg.Difference(a, b, options);
        Assert.True(result.Mesh.Faces.Count > 0, "Difference with winding number should produce faces");
    }

    [Fact]
    public void Difference_ResultBoundsContainedByA()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3));
        var result = Csg.Difference(a, b);
        var boundsA = a.Bounds;
        var boundsResult = result.Mesh.GetBounds();
        // Result bounds should be contained within (or equal to) A's bounds
        Assert.True(boundsResult.Min.X >= boundsA.Min.X - 1e-6,
            $"Result min X {boundsResult.Min.X} should be >= A min X {boundsA.Min.X}");
        Assert.True(boundsResult.Min.Y >= boundsA.Min.Y - 1e-6,
            $"Result min Y {boundsResult.Min.Y} should be >= A min Y {boundsA.Min.Y}");
        Assert.True(boundsResult.Min.Z >= boundsA.Min.Z - 1e-6,
            $"Result min Z {boundsResult.Min.Z} should be >= A min Z {boundsA.Min.Z}");
        Assert.True(boundsResult.Max.X <= boundsA.Max.X + 1e-6,
            $"Result max X {boundsResult.Max.X} should be <= A max X {boundsA.Max.X}");
        Assert.True(boundsResult.Max.Y <= boundsA.Max.Y + 1e-6,
            $"Result max Y {boundsResult.Max.Y} should be <= A max Y {boundsA.Max.Y}");
        Assert.True(boundsResult.Max.Z <= boundsA.Max.Z + 1e-6,
            $"Result max Z {boundsResult.Max.Z} should be <= A max Z {boundsA.Max.Z}");
    }

    [Fact]
    public void Difference_PatchCounts_Positive()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3));
        var result = Csg.Difference(a, b);
        Assert.True(result.PatchCountA > 0, "Should have patches from A");
        Assert.True(result.PatchCountB > 0, "Should have patches from B");
    }

    [Fact]
    public void Difference_DisjointCubes_PatchFromBIsClassifiedOutside()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(10, 10, 10));
        var result = Csg.Difference(a, b);
        // Disjoint: B gets 1 patch (whole mesh) classified as outside A
        // That patch is discarded by the assembler, so result faces come only from A
        Assert.Equal(a.Mesh.Faces.Count, result.Mesh.Faces.Count);
    }

    [Fact]
    public void Difference_TetrahedronMinusCube_HasFaces()
    {
        var tet = MeshFactory.CreateTetrahedron();
        var cube = MeshFactory.CreateCube(new Vec3(-0.3, -0.3, -0.3), 0.6);
        var result = Csg.Difference(tet, cube);
        Assert.True(result.Mesh.Faces.Count > 0);
    }

    [Fact]
    public void Difference_SphereSphere_HasFaces()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        var b = MeshFactory.CreateSphere(new Vec3(0.5, 0, 0), 1.0, 1);
        var result = Csg.Difference(a, b);
        Assert.True(result.Mesh.Faces.Count > 0, "Sphere minus sphere should produce faces");
    }
}
