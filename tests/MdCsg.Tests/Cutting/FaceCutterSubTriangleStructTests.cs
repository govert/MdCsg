using MdCsg.Cutting;
using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Cutting;

/// <summary>Phase 6: FaceCutter.SubTriangle struct — edge flags, vertex storage, record equality, cut pipeline</summary>
public class FaceCutterSubTriangleStructTests
{
    [Fact]
    public void EdgeFlags_AB()
    {
        var sub = new FaceCutter.SubTriangle(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, true, 0x01);
        Assert.True(sub.IsEdgeIntersection(0));
        Assert.False(sub.IsEdgeIntersection(1));
        Assert.False(sub.IsEdgeIntersection(2));
    }

    [Fact]
    public void EdgeFlags_BC()
    {
        var sub = new FaceCutter.SubTriangle(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, true, 0x02);
        Assert.False(sub.IsEdgeIntersection(0));
        Assert.True(sub.IsEdgeIntersection(1));
        Assert.False(sub.IsEdgeIntersection(2));
    }

    [Fact]
    public void EdgeFlags_CA()
    {
        var sub = new FaceCutter.SubTriangle(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, true, 0x04);
        Assert.False(sub.IsEdgeIntersection(0));
        Assert.False(sub.IsEdgeIntersection(1));
        Assert.True(sub.IsEdgeIntersection(2));
    }

    [Fact]
    public void EdgeFlags_AllSet()
    {
        var sub = new FaceCutter.SubTriangle(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, true, 0x07);
        Assert.True(sub.IsEdgeIntersection(0));
        Assert.True(sub.IsEdgeIntersection(1));
        Assert.True(sub.IsEdgeIntersection(2));
    }

    [Fact]
    public void EdgeFlags_NoneSet()
    {
        var sub = new FaceCutter.SubTriangle(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false, 0x00);
        Assert.False(sub.IsEdgeIntersection(0));
        Assert.False(sub.IsEdgeIntersection(1));
        Assert.False(sub.IsEdgeIntersection(2));
    }

    [Fact]
    public void HasIntersectionEdge_True_WhenFlagsNonZero()
    {
        var sub = new FaceCutter.SubTriangle(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, true, 0x01);
        Assert.True(sub.HasIntersectionEdge);
    }

    [Fact]
    public void HasIntersectionEdge_False_WhenFlagsZero()
    {
        var sub = new FaceCutter.SubTriangle(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0), 0, false, 0x00);
        Assert.False(sub.HasIntersectionEdge);
    }

    [Fact]
    public void OriginalFaceIndex_Stored()
    {
        var sub = new FaceCutter.SubTriangle(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0), 42, false, 0);
        Assert.Equal(42, sub.OriginalFaceIndex);
    }

    [Fact]
    public void Vertices_Stored()
    {
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        var c = new Vec3(7, 8, 9);
        var sub = new FaceCutter.SubTriangle(a, b, c, 0, false, 0);
        Assert.Equal(a, sub.A);
        Assert.Equal(b, sub.B);
        Assert.Equal(c, sub.C);
    }

    [Fact]
    public void MeshCutter_NoCutFaces_PassThrough()
    {
        var mesh = MeshFactory.CreateTetrahedron().Mesh;
        var empty = new Dictionary<int, List<IntersectionSegment>>();
        var result = MeshCutter.Cut(mesh, empty);
        Assert.Equal(4, result.SubTriangles.Count);
    }

    [Fact]
    public void MeshCutter_CutMesh_ProducesMore()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA, graph.FaceSegmentsA);
        Assert.True(cutA.SubTriangles.Count > meshA.Faces.Count);
    }

    [Fact]
    public void MeshCutter_AllOriginalFaces_Covered()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA, graph.FaceSegmentsA);
        var covered = new HashSet<int>();
        foreach (var sub in cutA.SubTriangles)
            covered.Add(sub.OriginalFaceIndex);
        Assert.Equal(meshA.Faces.Count, covered.Count);
    }

    [Fact]
    public void MeshCutter_SubTriangles_FiniteVertices()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA, graph.FaceSegmentsA);
        foreach (var sub in cutA.SubTriangles)
        {
            Assert.False(double.IsNaN(sub.A.X) || double.IsInfinity(sub.A.X));
            Assert.False(double.IsNaN(sub.B.X) || double.IsInfinity(sub.B.X));
            Assert.False(double.IsNaN(sub.C.X) || double.IsInfinity(sub.C.X));
        }
    }

    [Fact]
    public void MeshCutter_CutFaces_HaveIntersectionEdges()
    {
        var meshA = MeshFactory.CreateCube().Mesh;
        var meshB = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh;
        var graph = IntersectionGraph.Compute(meshA, meshB);
        var cutA = MeshCutter.Cut(meshA, graph.FaceSegmentsA);
        bool any = false;
        foreach (var sub in cutA.SubTriangles)
            if (sub.HasIntersectionEdge) { any = true; break; }
        Assert.True(any);
    }
}
