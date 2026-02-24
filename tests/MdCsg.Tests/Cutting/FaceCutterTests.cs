using MdCsg.Cutting;
using MdCsg.Intersection;
using MdCsg.Math;

namespace MdCsg.Tests.Cutting;

public class FaceCutterTests
{
    [Fact]
    public void NoSegments_ReturnsSingleTriangle()
    {
        var tri = new Triangle3(
            new Vec3(0, 0, 0),
            new Vec3(1, 0, 0),
            new Vec3(0, 1, 0));

        var result = FaceCutter.CutFace(tri, 0, []);
        Assert.Single(result);
        Assert.Equal(tri.A, result[0].A);
    }

    [Fact]
    public void SingleSegmentCrossing_ProducesMultipleTriangles()
    {
        var tri = new Triangle3(
            new Vec3(0, 0, 0),
            new Vec3(1, 0, 0),
            new Vec3(0, 1, 0));

        // A segment cutting through the triangle
        var seg = new IntersectionSegment(
            new Vec3(0.5, 0, 0),
            new Vec3(0, 0.5, 0),
            0, 1);

        var result = FaceCutter.CutFace(tri, 0, [seg]);
        Assert.True(result.Count >= 2, $"Expected at least 2 sub-triangles, got {result.Count}");

        // All sub-triangles should have the original face index
        foreach (var st in result)
            Assert.Equal(0, st.OriginalFaceIndex);
    }

    [Fact]
    public void SubTriangles_PreserveOriginalFaceIndex()
    {
        var tri = new Triangle3(
            new Vec3(0, 0, 0),
            new Vec3(2, 0, 0),
            new Vec3(0, 2, 0));

        var seg = new IntersectionSegment(
            new Vec3(1, 0, 0),
            new Vec3(0, 1, 0),
            5, 10);

        var result = FaceCutter.CutFace(tri, 5, [seg]);
        foreach (var st in result)
            Assert.Equal(5, st.OriginalFaceIndex);
    }

    [Fact]
    public void SegmentThroughVertex_StillProducesValidTriangles()
    {
        var tri = new Triangle3(
            new Vec3(0, 0, 0),
            new Vec3(1, 0, 0),
            new Vec3(0, 1, 0));

        // Segment from a vertex to the opposite edge
        var seg = new IntersectionSegment(
            new Vec3(0, 0, 0),
            new Vec3(0.5, 0.5, 0),
            0, 1);

        var result = FaceCutter.CutFace(tri, 0, [seg]);
        Assert.True(result.Count >= 1);
    }
}
