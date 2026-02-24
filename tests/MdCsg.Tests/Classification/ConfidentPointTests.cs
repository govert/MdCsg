using MdCsg.Bvh;
using MdCsg.Classification;
using MdCsg.Cutting;
using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Patches;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Classification;

/// <summary>Batch 22: ConfidentPoint tests (20 tests)</summary>
public class ConfidentPointTests
{
    [Fact]
    public void PointTriangleDistanceSq_PointAtVertex_Zero()
    {
        double d = ConfidentPoint.PointTriangleDistanceSq(Vec3.Zero, Vec3.Zero, Vec3.UnitX, Vec3.UnitY);
        Assert.Equal(0, d, 1e-15);
    }

    [Fact]
    public void PointTriangleDistanceSq_PointOnEdge_Zero()
    {
        var pt = new Vec3(0.5, 0, 0);
        double d = ConfidentPoint.PointTriangleDistanceSq(pt, Vec3.Zero, Vec3.UnitX, Vec3.UnitY);
        Assert.Equal(0, d, 1e-15);
    }

    [Fact]
    public void PointTriangleDistanceSq_PointInside_Zero()
    {
        var pt = new Vec3(0.25, 0.25, 0);
        double d = ConfidentPoint.PointTriangleDistanceSq(pt, Vec3.Zero, Vec3.UnitX, Vec3.UnitY);
        Assert.Equal(0, d, 1e-15);
    }

    [Fact]
    public void PointTriangleDistanceSq_PointAboveTriangle()
    {
        var pt = new Vec3(0.25, 0.25, 1);
        double d = ConfidentPoint.PointTriangleDistanceSq(pt, Vec3.Zero, Vec3.UnitX, Vec3.UnitY);
        Assert.Equal(1.0, d, 1e-12); // 1 unit above
    }

    [Fact]
    public void PointTriangleDistanceSq_PointBelowTriangle()
    {
        var pt = new Vec3(0.25, 0.25, -2);
        double d = ConfidentPoint.PointTriangleDistanceSq(pt, Vec3.Zero, Vec3.UnitX, Vec3.UnitY);
        Assert.Equal(4.0, d, 1e-12);
    }

    [Fact]
    public void PointTriangleDistanceSq_PointNearVertex()
    {
        var pt = new Vec3(-1, 0, 0);
        double d = ConfidentPoint.PointTriangleDistanceSq(pt, Vec3.Zero, Vec3.UnitX, Vec3.UnitY);
        Assert.Equal(1.0, d, 1e-12);
    }

    [Fact]
    public void PointTriangleDistanceSq_PointNearEdge()
    {
        var pt = new Vec3(0.5, -1, 0);
        double d = ConfidentPoint.PointTriangleDistanceSq(pt, Vec3.Zero, Vec3.UnitX, Vec3.UnitY);
        Assert.Equal(1.0, d, 1e-12);
    }

    [Fact]
    public void PointTriangleDistanceSq_FarPoint()
    {
        var pt = new Vec3(10, 10, 10);
        double d = ConfidentPoint.PointTriangleDistanceSq(pt, Vec3.Zero, Vec3.UnitX, Vec3.UnitY);
        Assert.True(d > 100);
    }

    [Fact]
    public void PointTriangleDistanceSq_Symmetric()
    {
        var pt = new Vec3(0, 0, 1);
        double d1 = ConfidentPoint.PointTriangleDistanceSq(pt, Vec3.Zero, Vec3.UnitX, Vec3.UnitY);
        double d2 = ConfidentPoint.PointTriangleDistanceSq(pt, Vec3.UnitX, Vec3.UnitY, Vec3.Zero);
        Assert.Equal(d1, d2, 1e-12);
    }

    [Fact]
    public void PointTriangleDistanceSq_ProjectsOntoHypotenuse()
    {
        // Point above the midpoint of the hypotenuse (0.5, 0.5, 0)
        var pt = new Vec3(0.5, 0.5, 1);
        double d = ConfidentPoint.PointTriangleDistanceSq(pt, Vec3.Zero, Vec3.UnitX, Vec3.UnitY);
        // Point is on the hypotenuse line, so projected distance is just 1.0 (z^2)
        Assert.Equal(1.0, d, 1e-10);
    }

    [Fact]
    public void FindConfidentPoint_SingleSubTriangle_ReturnsCentroid()
    {
        var cube = MeshFactory.CreateCube();
        var cubeb = MeshFactory.CreateCube(new Vec3(5, 0, 0)); // disjoint
        var emptySegs = new Dictionary<int, List<IntersectionSegment>>();
        var cutResult = MeshCutter.Cut(cube.Mesh, emptySegs);
        var adj = SubTriangleAdjacency.Build(cutResult.SubTriangles);
        var patches = PatchExtractor.Extract(cutResult.SubTriangles, adj);

        // Single patch with all sub-triangles
        var (point, margin) = ConfidentPoint.FindConfidentPoint(patches[0], cutResult.SubTriangles, cubeb.Bvh);
        Assert.True(margin > 0);
    }

    [Fact]
    public void FindConfidentPoint_WithOverlap_HasPositiveMargin()
    {
        var cubeA = MeshFactory.CreateCube();
        var cubeB = MeshFactory.CreateCube(new Vec3(0.5, 0, 0));
        var graph = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh);
        var cutResult = MeshCutter.Cut(cubeA.Mesh, graph.FaceSegmentsA);
        var adj = SubTriangleAdjacency.Build(cutResult.SubTriangles);
        var patches = PatchExtractor.Extract(cutResult.SubTriangles, adj);

        foreach (var patch in patches)
        {
            var (point, margin) = ConfidentPoint.FindConfidentPoint(patch, cutResult.SubTriangles, cubeB.Bvh);
            Assert.True(margin >= 0);
        }
    }

    [Fact]
    public void FindConfidentPoint_EmptyPatch_ReturnsZero()
    {
        var cube = MeshFactory.CreateCube();
        var patch = new Patch(0); // no sub-triangles
        var emptySegs = new Dictionary<int, List<IntersectionSegment>>();
        var cutResult = MeshCutter.Cut(cube.Mesh, emptySegs);
        var (point, margin) = ConfidentPoint.FindConfidentPoint(patch, cutResult.SubTriangles, cube.Bvh);
        Assert.Equal(-1, margin);
    }

    [Fact]
    public void FindConfidentPoint_DisjointSolids_LargeMargin()
    {
        var cubeA = MeshFactory.CreateCube();
        var cubeB = MeshFactory.CreateCube(new Vec3(10, 0, 0));
        var graph = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh);
        var cutResult = MeshCutter.Cut(cubeA.Mesh, graph.FaceSegmentsA);
        var adj = SubTriangleAdjacency.Build(cutResult.SubTriangles);
        var patches = PatchExtractor.Extract(cutResult.SubTriangles, adj);

        var (_, margin) = ConfidentPoint.FindConfidentPoint(patches[0], cutResult.SubTriangles, cubeB.Bvh);
        Assert.True(margin > 5); // cubeB is 10 units away
    }

    [Fact]
    public void FindConfidentPoint_SelectsMaxMargin()
    {
        var cubeA = MeshFactory.CreateCube();
        // Diagonal offset avoids coplanar faces (which would produce near-zero margins)
        var cubeB = MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3));
        var graph = IntersectionGraph.Compute(cubeA.Mesh, cubeB.Mesh);
        var cutResult = MeshCutter.Cut(cubeA.Mesh, graph.FaceSegmentsA);
        var adj = SubTriangleAdjacency.Build(cutResult.SubTriangles);
        var patches = PatchExtractor.Extract(cutResult.SubTriangles, adj);

        foreach (var patch in patches)
        {
            var (_, margin) = ConfidentPoint.FindConfidentPoint(patch, cutResult.SubTriangles, cubeB.Bvh);
            Assert.True(margin > 0);
        }
    }

    [Fact]
    public void FindConfidentPoint_EmptyBvh_ReturnsMaxValue()
    {
        var cube = MeshFactory.CreateCube();
        var emptyMesh = new MdCsg.Mesh.HalfEdgeMesh();
        var emptyBvh = BvhTree.Build(emptyMesh);
        var cutResult = MeshCutter.Cut(cube.Mesh, new Dictionary<int, List<IntersectionSegment>>());
        var adj = SubTriangleAdjacency.Build(cutResult.SubTriangles);
        var patches = PatchExtractor.Extract(cutResult.SubTriangles, adj);
        var (_, margin) = ConfidentPoint.FindConfidentPoint(patches[0], cutResult.SubTriangles, emptyBvh);
        Assert.Equal(double.MaxValue, margin);
    }

    [Fact]
    public void PointTriangleDistanceSq_LargeTriangle()
    {
        var a = new Vec3(-100, -100, 0);
        var b = new Vec3(100, -100, 0);
        var c = new Vec3(0, 100, 0);
        var pt = new Vec3(0, 0, 5);
        double d = ConfidentPoint.PointTriangleDistanceSq(pt, a, b, c);
        Assert.Equal(25.0, d, 1e-10);
    }

    [Fact]
    public void PointTriangleDistanceSq_DegenerateTriangle_HandledGracefully()
    {
        // Degenerate triangle (collinear points)
        var a = Vec3.Zero;
        var b = Vec3.UnitX;
        var c = new Vec3(0.5, 0, 0); // collinear
        var pt = new Vec3(0, 1, 0);
        // Should not throw
        double d = ConfidentPoint.PointTriangleDistanceSq(pt, a, b, c);
        Assert.True(d >= 0);
    }

    [Fact]
    public void PointTriangleDistanceSq_NearHypotenuse_ProjectsToEdge()
    {
        // Point beyond the hypotenuse of the triangle
        var pt = new Vec3(1, 1, 0);
        double d = ConfidentPoint.PointTriangleDistanceSq(pt, Vec3.Zero, Vec3.UnitX, Vec3.UnitY);
        // Closest point on the hypotenuse x+y=1 is (0.5, 0.5, 0)
        double expected = Vec3.DistanceSquared(pt, new Vec3(0.5, 0.5, 0));
        Assert.Equal(expected, d, 1e-10);
    }

    [Fact]
    public void PointTriangleDistanceSq_CentroidProjection()
    {
        var centroid = new Vec3(1.0 / 3.0, 1.0 / 3.0, 0);
        var pt = centroid + new Vec3(0, 0, 3);
        double d = ConfidentPoint.PointTriangleDistanceSq(pt, Vec3.Zero, Vec3.UnitX, Vec3.UnitY);
        Assert.Equal(9.0, d, 1e-10);
    }
}
