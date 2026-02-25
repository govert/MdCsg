using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Mesh;

/// <summary>Phase 6: MeshBuilder indexed mesh construction, twin linking, and welding tests</summary>
public class MeshBuilderIndexedTests
{
    [Fact]
    public void Build_FromTriangles_SingleTriangle()
    {
        var builder = new MeshBuilder();
        var triangles = new List<Triangle3>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0))
        };
        var mesh = builder.Build(triangles);
        Assert.Equal(3, mesh.Vertices.Count);
        Assert.Single(mesh.Faces);
        Assert.Equal(3, mesh.HalfEdges.Count);
    }

    [Fact]
    public void Build_FromTriangles_TwoTriangles_SharedEdge()
    {
        var builder = new MeshBuilder();
        var triangles = new List<Triangle3>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new(new Vec3(1, 0, 0), new Vec3(1, 1, 0), new Vec3(0, 1, 0))
        };
        var mesh = builder.Build(triangles);
        Assert.Equal(4, mesh.Vertices.Count);
        Assert.Equal(2, mesh.Faces.Count);
    }

    [Fact]
    public void Build_FromTriangles_SharedEdge_TwinsLinked()
    {
        var builder = new MeshBuilder();
        var triangles = new List<Triangle3>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new(new Vec3(1, 0, 0), new Vec3(1, 1, 0), new Vec3(0, 1, 0))
        };
        var mesh = builder.Build(triangles);
        // At least some half-edges should have twins (shared edge B-C / C-B)
        int twinnedCount = mesh.HalfEdges.Count(he => he.Twin != null);
        Assert.True(twinnedCount >= 2, $"Expected at least 2 twinned half-edges, got {twinnedCount}");
    }

    [Fact]
    public void Build_FromIndexed_SingleTriangle()
    {
        var builder = new MeshBuilder();
        var positions = new Vec3[]
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 1, 0)
        };
        var indices = new (int, int, int)[] { (0, 1, 2) };
        var mesh = builder.Build(positions, indices);
        Assert.Equal(3, mesh.Vertices.Count);
        Assert.Single(mesh.Faces);
    }

    [Fact]
    public void Build_FromIndexed_TwoTriangles()
    {
        var builder = new MeshBuilder();
        var positions = new Vec3[]
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 1, 0), new(1, 1, 0)
        };
        var indices = new (int, int, int)[] { (0, 1, 2), (1, 3, 2) };
        var mesh = builder.Build(positions, indices);
        Assert.Equal(4, mesh.Vertices.Count);
        Assert.Equal(2, mesh.Faces.Count);
    }

    [Fact]
    public void Build_FromIndexed_ExactVertexCount()
    {
        var builder = new MeshBuilder();
        var positions = new Vec3[]
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 1, 0), new(1, 1, 0)
        };
        var indices = new (int, int, int)[] { (0, 1, 2), (1, 3, 2) };
        var mesh = builder.Build(positions, indices);
        // Indexed build does NOT weld — exact number of positions
        Assert.Equal(positions.Length, mesh.Vertices.Count);
    }

    [Fact]
    public void Build_FromTriangles_WeldingMergesCloseVertices()
    {
        var builder = new MeshBuilder(weldTolerance: 1e-6);
        // Two triangles sharing an edge with slightly different vertex positions
        var triangles = new List<Triangle3>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new(new Vec3(1, 1e-8, 1e-8), new Vec3(1, 1, 0), new Vec3(0, 1, 1e-8))
        };
        var mesh = builder.Build(triangles);
        // With welding, close vertices should merge
        Assert.True(mesh.Vertices.Count <= 5, $"Expected ≤5 vertices after welding, got {mesh.Vertices.Count}");
    }

    [Fact]
    public void Build_EmptyTriangles_EmptyMesh()
    {
        var builder = new MeshBuilder();
        var mesh = builder.Build(new List<Triangle3>());
        Assert.Empty(mesh.Vertices);
        Assert.Empty(mesh.Faces);
        Assert.Empty(mesh.HalfEdges);
    }

    [Fact]
    public void Build_Cube_12Faces()
    {
        var builder = new MeshBuilder();
        var cube = MeshFactory.CreateCube();
        // Already built by MeshFactory, but verify the structure
        Assert.Equal(12, cube.Mesh.Faces.Count);
        Assert.Equal(8, cube.Mesh.Vertices.Count);
    }

    [Fact]
    public void Build_FaceCycles_Valid()
    {
        var builder = new MeshBuilder();
        var triangles = new List<Triangle3>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new(new Vec3(1, 0, 0), new Vec3(1, 1, 0), new Vec3(0, 1, 0))
        };
        var mesh = builder.Build(triangles);
        Assert.True(MeshValidator.HasValidFaceCycles(mesh));
    }

    [Fact]
    public void Build_TwinSymmetry()
    {
        var builder = new MeshBuilder();
        var triangles = new List<Triangle3>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new(new Vec3(1, 0, 0), new Vec3(1, 1, 0), new Vec3(0, 1, 0))
        };
        var mesh = builder.Build(triangles);
        foreach (var he in mesh.HalfEdges)
        {
            if (he.Twin != null)
                Assert.Same(he, he.Twin.Twin);
        }
    }

    [Fact]
    public void HalfEdgeMesh_AddVertex_IncrementsId()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(new Vec3(0, 0, 0));
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        Assert.Equal(0, v0.Id);
        Assert.Equal(1, v1.Id);
    }

    [Fact]
    public void HalfEdgeMesh_AddHalfEdge_IncrementsId()
    {
        var mesh = new HalfEdgeMesh();
        var he0 = mesh.AddHalfEdge();
        var he1 = mesh.AddHalfEdge();
        Assert.Equal(0, he0.Id);
        Assert.Equal(1, he1.Id);
    }

    [Fact]
    public void HalfEdgeMesh_GetBounds_Empty_IsEmpty()
    {
        var mesh = new HalfEdgeMesh();
        var bounds = mesh.GetBounds();
        Assert.Equal(Aabb.Empty, bounds);
    }

    [Fact]
    public void HalfEdgeMesh_GetBounds_SingleVertex()
    {
        var mesh = new HalfEdgeMesh();
        mesh.AddVertex(new Vec3(5, 10, 15));
        var bounds = mesh.GetBounds();
        Assert.Equal(5, bounds.Min.X, 1e-10);
        Assert.Equal(10, bounds.Min.Y, 1e-10);
        Assert.Equal(15, bounds.Min.Z, 1e-10);
    }
}
