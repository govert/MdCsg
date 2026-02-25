using MdCsg.Cutting;
using MdCsg.Intersection;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Cutting;

/// <summary>Phase 6: MeshCutter — Cut pipeline, CutResult, face mapping, empty/single/multi-segment scenarios</summary>
public class MeshCutterPipelinePropertyTests
{
    [Fact]
    public void Cut_NoSegments_PreservesFaces()
    {
        var cube = MeshFactory.CreateCube();
        var emptySegments = new Dictionary<int, List<IntersectionSegment>>();
        var result = MeshCutter.Cut(cube.Mesh, emptySegments);
        Assert.Equal(cube.Mesh.Faces.Count, result.SubTriangles.Count);
    }

    [Fact]
    public void Cut_NoSegments_AllSubTrianglesHaveNoIntersectionEdge()
    {
        var cube = MeshFactory.CreateCube();
        var emptySegments = new Dictionary<int, List<IntersectionSegment>>();
        var result = MeshCutter.Cut(cube.Mesh, emptySegments);
        foreach (var st in result.SubTriangles)
            Assert.False(st.HasIntersectionEdge);
    }

    [Fact]
    public void Cut_NoSegments_OriginalFaceIndicesAreSequential()
    {
        var cube = MeshFactory.CreateCube();
        var emptySegments = new Dictionary<int, List<IntersectionSegment>>();
        var result = MeshCutter.Cut(cube.Mesh, emptySegments);
        for (int i = 0; i < result.SubTriangles.Count; i++)
            Assert.Equal(i, result.SubTriangles[i].OriginalFaceIndex);
    }

    [Fact]
    public void Cut_NoSegments_MeshHasSameFaceCount()
    {
        var cube = MeshFactory.CreateCube();
        var emptySegments = new Dictionary<int, List<IntersectionSegment>>();
        var result = MeshCutter.Cut(cube.Mesh, emptySegments);
        Assert.Equal(cube.Mesh.Faces.Count, result.Mesh.Faces.Count);
    }

    [Fact]
    public void CutResult_HasMesh()
    {
        var cube = MeshFactory.CreateCube();
        var emptySegments = new Dictionary<int, List<IntersectionSegment>>();
        var result = MeshCutter.Cut(cube.Mesh, emptySegments);
        Assert.NotNull(result.Mesh);
        Assert.True(result.Mesh.Faces.Count > 0);
    }

    [Fact]
    public void CutResult_HasSubTriangles()
    {
        var cube = MeshFactory.CreateCube();
        var emptySegments = new Dictionary<int, List<IntersectionSegment>>();
        var result = MeshCutter.Cut(cube.Mesh, emptySegments);
        Assert.NotNull(result.SubTriangles);
        Assert.True(result.SubTriangles.Count > 0);
    }

    [Fact]
    public void Cut_WithSegment_ProducesMoreSubTriangles()
    {
        var cube = MeshFactory.CreateCube();
        // Add a segment to face 0
        var face0 = cube.Mesh.Faces[0];
        face0.GetTrianglePositions(out var va, out var vb, out var vc);
        var mid_ab = (va + vb) * 0.5;
        var mid_bc = (vb + vc) * 0.5;
        var seg = new IntersectionSegment(mid_ab, mid_bc, 0, 1);
        var faceSegments = new Dictionary<int, List<IntersectionSegment>>
        {
            { 0, new List<IntersectionSegment> { seg } }
        };
        var result = MeshCutter.Cut(cube.Mesh, faceSegments);
        Assert.True(result.SubTriangles.Count > cube.Mesh.Faces.Count,
            $"Cutting should produce more sub-triangles: {result.SubTriangles.Count} vs {cube.Mesh.Faces.Count}");
    }

    [Fact]
    public void Cut_WithSegment_SomeST_HaveIntersectionEdge()
    {
        var cube = MeshFactory.CreateCube();
        var face0 = cube.Mesh.Faces[0];
        face0.GetTrianglePositions(out var va, out var vb, out var vc);
        var mid_ab = (va + vb) * 0.5;
        var mid_bc = (vb + vc) * 0.5;
        var seg = new IntersectionSegment(mid_ab, mid_bc, 0, 1);
        var faceSegments = new Dictionary<int, List<IntersectionSegment>>
        {
            { 0, new List<IntersectionSegment> { seg } }
        };
        var result = MeshCutter.Cut(cube.Mesh, faceSegments);
        Assert.True(result.SubTriangles.Any(st => st.HasIntersectionEdge),
            "Some sub-triangles should have intersection edges");
    }

    [Fact]
    public void Cut_WithSegment_AllOriginalFaceIdsValid()
    {
        var cube = MeshFactory.CreateCube();
        var face0 = cube.Mesh.Faces[0];
        face0.GetTrianglePositions(out var va, out var vb, out var vc);
        var mid_ab = (va + vb) * 0.5;
        var mid_bc = (vb + vc) * 0.5;
        var seg = new IntersectionSegment(mid_ab, mid_bc, 0, 1);
        var faceSegments = new Dictionary<int, List<IntersectionSegment>>
        {
            { 0, new List<IntersectionSegment> { seg } }
        };
        var result = MeshCutter.Cut(cube.Mesh, faceSegments);
        foreach (var st in result.SubTriangles)
        {
            Assert.True(st.OriginalFaceIndex >= 0 && st.OriginalFaceIndex < cube.Mesh.Faces.Count,
                $"OriginalFaceIndex {st.OriginalFaceIndex} out of range");
        }
    }

    [Fact]
    public void Cut_WithSegment_CutFaceProducesMoreForFace0()
    {
        var cube = MeshFactory.CreateCube();
        var face0 = cube.Mesh.Faces[0];
        face0.GetTrianglePositions(out var va, out var vb, out var vc);
        var mid_ab = (va + vb) * 0.5;
        var mid_bc = (vb + vc) * 0.5;
        var seg = new IntersectionSegment(mid_ab, mid_bc, 0, 1);
        var faceSegments = new Dictionary<int, List<IntersectionSegment>>
        {
            { 0, new List<IntersectionSegment> { seg } }
        };
        var result = MeshCutter.Cut(cube.Mesh, faceSegments);
        int face0SubTriangles = result.SubTriangles.Count(st => st.OriginalFaceIndex == 0);
        Assert.True(face0SubTriangles >= 2,
            $"Face 0 should have at least 2 sub-triangles after cut, got {face0SubTriangles}");
    }

    [Fact]
    public void Cut_WithSegment_UncutFacesGetOneSubTriangle()
    {
        var cube = MeshFactory.CreateCube();
        var face0 = cube.Mesh.Faces[0];
        face0.GetTrianglePositions(out var va, out var vb, out var vc);
        var mid_ab = (va + vb) * 0.5;
        var mid_bc = (vb + vc) * 0.5;
        var seg = new IntersectionSegment(mid_ab, mid_bc, 0, 1);
        var faceSegments = new Dictionary<int, List<IntersectionSegment>>
        {
            { 0, new List<IntersectionSegment> { seg } }
        };
        var result = MeshCutter.Cut(cube.Mesh, faceSegments);
        // Faces 1..11 should each get exactly 1 sub-triangle
        for (int f = 1; f < cube.Mesh.Faces.Count; f++)
        {
            int count = result.SubTriangles.Count(st => st.OriginalFaceIndex == f);
            Assert.Equal(1, count);
        }
    }

    [Fact]
    public void Cut_Tetrahedron_NoSegments_FourSubTriangles()
    {
        var tet = MeshFactory.CreateTetrahedron();
        var empty = new Dictionary<int, List<IntersectionSegment>>();
        var result = MeshCutter.Cut(tet.Mesh, empty);
        Assert.Equal(4, result.SubTriangles.Count);
    }

    [Fact]
    public void Cut_Sphere_NoSegments_PreservesFaceCount()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var empty = new Dictionary<int, List<IntersectionSegment>>();
        var result = MeshCutter.Cut(sphere.Mesh, empty);
        Assert.Equal(sphere.Mesh.Faces.Count, result.SubTriangles.Count);
    }

    [Fact]
    public void Cut_OutputMesh_HasFaces()
    {
        var cube = MeshFactory.CreateCube();
        var face0 = cube.Mesh.Faces[0];
        face0.GetTrianglePositions(out var va, out var vb, out var vc);
        var mid_ab = (va + vb) * 0.5;
        var mid_bc = (vb + vc) * 0.5;
        var seg = new IntersectionSegment(mid_ab, mid_bc, 0, 1);
        var faceSegments = new Dictionary<int, List<IntersectionSegment>>
        {
            { 0, new List<IntersectionSegment> { seg } }
        };
        var result = MeshCutter.Cut(cube.Mesh, faceSegments);
        Assert.True(result.Mesh.Faces.Count >= cube.Mesh.Faces.Count);
    }
}
