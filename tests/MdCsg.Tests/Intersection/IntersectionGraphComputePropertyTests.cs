using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Intersection;

/// <summary>Phase 6: IntersectionGraph.Compute — segments, face segment maps, coplanar tracking</summary>
public class IntersectionGraphComputePropertyTests
{
    [Fact]
    public void Compute_DisjointCubes_EmptySegments()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(5, 0, 0));
        var result = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        Assert.Equal(0, result.Segments.Count);
    }

    [Fact]
    public void Compute_OverlappingCubes_HasSegments()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var result = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        Assert.True(result.Segments.Count > 0, "Overlapping cubes should produce intersection segments");
    }

    [Fact]
    public void Compute_OverlappingCubes_FaceSegmentsA_NotEmpty()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var result = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        Assert.True(result.FaceSegmentsA.Count > 0);
    }

    [Fact]
    public void Compute_OverlappingCubes_FaceSegmentsB_NotEmpty()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var result = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        Assert.True(result.FaceSegmentsB.Count > 0);
    }

    [Fact]
    public void Compute_DisjointCubes_EmptyFaceSegments()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(5, 0, 0));
        var result = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        Assert.Equal(0, result.FaceSegmentsA.Count);
        Assert.Equal(0, result.FaceSegmentsB.Count);
    }

    [Fact]
    public void Compute_CubeSphere_HasSegments()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero);
        var sphere = MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.8, 2);
        var result = IntersectionGraph.Compute(cube.Mesh, sphere.Mesh);
        Assert.True(result.Segments.Count > 0);
    }

    [Fact]
    public void Compute_SphereSphere_Overlapping_HasSegments()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var b = MeshFactory.CreateSphere(new Vec3(1.0, 0, 0), 1.0, 2);
        var result = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        Assert.True(result.Segments.Count > 0);
    }

    [Fact]
    public void Compute_SphereSphere_Disjoint_NoSegments()
    {
        var a = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var b = MeshFactory.CreateSphere(new Vec3(10, 0, 0), 1.0, 2);
        var result = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        Assert.Equal(0, result.Segments.Count);
    }

    [Fact]
    public void Compute_Segments_AreNonDegenerate()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var result = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        foreach (var seg in result.Segments)
            Assert.False(seg.IsDegenerate, $"Segment should not be degenerate: length={seg.Length}");
    }

    [Fact]
    public void Compute_Segments_HavePositiveLength()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var result = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        foreach (var seg in result.Segments)
            Assert.True(seg.Length > 0, "Intersection segment should have positive length");
    }

    [Fact]
    public void Compute_FaceSegments_ValidFaceIndices()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var result = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        foreach (var kvp in result.FaceSegmentsA)
            Assert.True(kvp.Key >= 0 && kvp.Key < a.Mesh.Faces.Count);
        foreach (var kvp in result.FaceSegmentsB)
            Assert.True(kvp.Key >= 0 && kvp.Key < b.Mesh.Faces.Count);
    }

    [Fact]
    public void Compute_CustomGridSize_Works()
    {
        var a = MeshFactory.CreateCube(Vec3.Zero);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var result = IntersectionGraph.Compute(a.Mesh, b.Mesh, 1e-6);
        Assert.True(result.Segments.Count > 0);
    }

    [Fact]
    public void Compute_PartiallyOverlappingCubes_HasSegments()
    {
        // Cubes that partially overlap — faces should cross and produce segments
        var a = MeshFactory.CreateCube(Vec3.Zero, 1.0);
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5), 1.0);
        var result = IntersectionGraph.Compute(a.Mesh, b.Mesh);
        Assert.True(result.Segments.Count > 0, "Partially overlapping cubes should produce segments");
    }
}
