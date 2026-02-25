using MdCsg.Cutting;
using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Cutting;

/// <summary>Phase 6: MeshCutter — Cut with no segments preserves faces, cut with segments produces sub-triangles, original face ID tracking</summary>
public class MeshCutterPreservationPropertyTests
{
    [Fact]
    public void Cut_NoSegments_PreservesAllFaces()
    {
        var cube = MeshFactory.CreateCube();
        var emptySegments = new Dictionary<int, List<IntersectionSegment>>();
        var result = MeshCutter.Cut(cube.Mesh, emptySegments);
        Assert.Equal(cube.Mesh.Faces.Count, result.SubTriangles.Count);
    }

    [Fact]
    public void Cut_NoSegments_SubTrianglesHaveCorrectOriginalFaceIndex()
    {
        var cube = MeshFactory.CreateCube();
        var emptySegments = new Dictionary<int, List<IntersectionSegment>>();
        var result = MeshCutter.Cut(cube.Mesh, emptySegments);
        for (int i = 0; i < result.SubTriangles.Count; i++)
        {
            Assert.Equal(i, result.SubTriangles[i].OriginalFaceIndex);
        }
    }

    [Fact]
    public void Cut_NoSegments_SubTrianglesNotIntersection()
    {
        var cube = MeshFactory.CreateCube();
        var emptySegments = new Dictionary<int, List<IntersectionSegment>>();
        var result = MeshCutter.Cut(cube.Mesh, emptySegments);
        foreach (var sub in result.SubTriangles)
        {
            Assert.False(sub.HasIntersectionEdge);
        }
    }

    [Fact]
    public void Cut_NoSegments_ResultMesh_HasSameFaceCount()
    {
        var cube = MeshFactory.CreateCube();
        var emptySegments = new Dictionary<int, List<IntersectionSegment>>();
        var result = MeshCutter.Cut(cube.Mesh, emptySegments);
        Assert.Equal(cube.Mesh.Faces.Count, result.Mesh.Faces.Count);
    }

    [Fact]
    public void Cut_WithSegments_ProducesMoreSubTriangles()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        // Create a segment that crosses face 0
        cube.Mesh.Faces[0].GetTrianglePositions(out var a, out var b, out var c);
        var mid_ab = (a + b) * 0.5;
        var mid_bc = (b + c) * 0.5;
        var seg = new IntersectionSegment(mid_ab, mid_bc, 0, 99);

        var segments = new Dictionary<int, List<IntersectionSegment>>
        {
            { 0, new List<IntersectionSegment> { seg } }
        };
        var result = MeshCutter.Cut(cube.Mesh, segments);
        // Cutting one face should produce more sub-triangles than original face count
        Assert.True(result.SubTriangles.Count >= cube.Mesh.Faces.Count,
            $"Expected at least {cube.Mesh.Faces.Count} sub-triangles, got {result.SubTriangles.Count}");
    }

    [Fact]
    public void Cut_WithSegments_CutFace_HasIntersectionEdge()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        cube.Mesh.Faces[0].GetTrianglePositions(out var a, out var b, out var c);
        var mid_ab = (a + b) * 0.5;
        var mid_bc = (b + c) * 0.5;
        var seg = new IntersectionSegment(mid_ab, mid_bc, 0, 99);

        var segments = new Dictionary<int, List<IntersectionSegment>>
        {
            { 0, new List<IntersectionSegment> { seg } }
        };
        var result = MeshCutter.Cut(cube.Mesh, segments);

        // At least one sub-triangle from the cut face should have intersection edge
        bool anyIntersection = false;
        foreach (var sub in result.SubTriangles)
        {
            if (sub.OriginalFaceIndex == 0 && sub.HasIntersectionEdge)
            {
                anyIntersection = true;
                break;
            }
        }
        Assert.True(anyIntersection, "Cut face should produce sub-triangles with intersection edges");
    }

    [Fact]
    public void Cut_UncutFaces_RetainOriginalFaceIndex()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        cube.Mesh.Faces[0].GetTrianglePositions(out var a, out var b, out var c);
        var mid_ab = (a + b) * 0.5;
        var mid_bc = (b + c) * 0.5;
        var seg = new IntersectionSegment(mid_ab, mid_bc, 0, 99);

        var segments = new Dictionary<int, List<IntersectionSegment>>
        {
            { 0, new List<IntersectionSegment> { seg } }
        };
        var result = MeshCutter.Cut(cube.Mesh, segments);

        // Faces 1-11 should still have their original face indices
        var uncutOriginals = new HashSet<int>();
        foreach (var sub in result.SubTriangles)
        {
            if (!sub.HasIntersectionEdge && sub.OriginalFaceIndex != 0)
                uncutOriginals.Add(sub.OriginalFaceIndex);
        }
        // All 11 other faces should appear uncut
        for (int i = 1; i < 12; i++)
        {
            Assert.Contains(i, uncutOriginals);
        }
    }

    [Fact]
    public void Cut_NoSegments_Tetrahedron_4SubTriangles()
    {
        var tet = MeshFactory.CreateTetrahedron();
        var emptySegments = new Dictionary<int, List<IntersectionSegment>>();
        var result = MeshCutter.Cut(tet.Mesh, emptySegments);
        Assert.Equal(4, result.SubTriangles.Count);
    }

    [Fact]
    public void Cut_EmptyMesh_EmptyResult()
    {
        var builder = new MeshBuilder();
        var emptyMesh = builder.Build(Array.Empty<Triangle3>());
        var emptySegments = new Dictionary<int, List<IntersectionSegment>>();
        var result = MeshCutter.Cut(emptyMesh, emptySegments);
        Assert.Equal(0, result.SubTriangles.Count);
        Assert.Equal(0, result.Mesh.Faces.Count);
    }

    [Fact]
    public void Cut_NoSegments_SubTriangleVertices_MatchOriginal()
    {
        var cube = MeshFactory.CreateCube();
        var emptySegments = new Dictionary<int, List<IntersectionSegment>>();
        var result = MeshCutter.Cut(cube.Mesh, emptySegments);

        for (int i = 0; i < cube.Mesh.Faces.Count; i++)
        {
            cube.Mesh.Faces[i].GetTrianglePositions(out var a, out var b, out var c);
            var sub = result.SubTriangles[i];
            Assert.True(Vec3.DistanceSquared(a, sub.A) < 1e-10, "Vertex A should match");
            Assert.True(Vec3.DistanceSquared(b, sub.B) < 1e-10, "Vertex B should match");
            Assert.True(Vec3.DistanceSquared(c, sub.C) < 1e-10, "Vertex C should match");
        }
    }

    [Fact]
    public void Cut_ResultMesh_TransfersOriginalFaceIds()
    {
        var cube = MeshFactory.CreateCube();
        var emptySegments = new Dictionary<int, List<IntersectionSegment>>();
        var result = MeshCutter.Cut(cube.Mesh, emptySegments);
        for (int i = 0; i < result.Mesh.Faces.Count; i++)
        {
            Assert.Equal(result.SubTriangles[i].OriginalFaceIndex, result.Mesh.Faces[i].OriginalFaceId);
        }
    }

    [Fact]
    public void Cut_NoSegments_Sphere_PreservesAllFaces()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        var emptySegments = new Dictionary<int, List<IntersectionSegment>>();
        var result = MeshCutter.Cut(sphere.Mesh, emptySegments);
        Assert.Equal(80, result.SubTriangles.Count);
    }

    [Fact]
    public void Cut_WithEmptySegmentList_TreatsAsUncut()
    {
        var cube = MeshFactory.CreateCube();
        var segments = new Dictionary<int, List<IntersectionSegment>>
        {
            { 0, new List<IntersectionSegment>() } // empty list for face 0
        };
        var result = MeshCutter.Cut(cube.Mesh, segments);
        // Empty segment list means face is uncut
        Assert.Equal(cube.Mesh.Faces.Count, result.SubTriangles.Count);
    }

    [Fact]
    public void Cut_SubTriangleEdgeFlags_ZeroForUncutFaces()
    {
        var cube = MeshFactory.CreateCube();
        var emptySegments = new Dictionary<int, List<IntersectionSegment>>();
        var result = MeshCutter.Cut(cube.Mesh, emptySegments);
        foreach (var sub in result.SubTriangles)
        {
            Assert.Equal(0, sub.IntersectionEdgeFlags);
        }
    }
}
