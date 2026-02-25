using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Tests.Mesh;

/// <summary>Phase 6: MeshBuilder indexed construction — Build(positions, indices), twin linking, vertex count</summary>
public class MeshBuilderIndexedConstructionTests
{
    [Fact]
    public void SingleTriangle_Indexed()
    {
        var positions = new Vec3[] { Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0) };
        var indices = new (int, int, int)[] { (0, 1, 2) };
        var mesh = new MeshBuilder().Build(positions, indices);
        Assert.Equal(3, mesh.Vertices.Count);
        Assert.Equal(1, mesh.Faces.Count);
    }

    [Fact]
    public void TwoTriangles_SharedEdge_TwinsLinked()
    {
        var positions = new Vec3[]
        {
            Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0), new Vec3(1, 1, 0)
        };
        var indices = new (int, int, int)[] { (0, 1, 2), (1, 3, 2) };
        var mesh = new MeshBuilder().Build(positions, indices);
        Assert.Equal(4, mesh.Vertices.Count);
        Assert.Equal(2, mesh.Faces.Count);
        // Shared edge between vertices 1 and 2 should have twins
        int twinnedCount = 0;
        foreach (var he in mesh.HalfEdges)
        {
            if (he.Twin != null) twinnedCount++;
        }
        Assert.True(twinnedCount > 0);
    }

    [Fact]
    public void VertexCount_ExactlyAsProvided()
    {
        var positions = new Vec3[] { new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), new Vec3(0, 0, 1) };
        var indices = new (int, int, int)[] { (0, 1, 2), (0, 2, 3), (0, 3, 1), (1, 3, 2) };
        var mesh = new MeshBuilder().Build(positions, indices);
        Assert.Equal(4, mesh.Vertices.Count);
        Assert.Equal(4, mesh.Faces.Count);
    }

    [Fact]
    public void Tetrahedron_Indexed_AllTwinsLinked()
    {
        var positions = new Vec3[]
        {
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), new Vec3(0, 0, 1)
        };
        var indices = new (int, int, int)[] { (0, 2, 1), (0, 1, 3), (0, 3, 2), (1, 2, 3) };
        var mesh = new MeshBuilder().Build(positions, indices);
        Assert.True(MeshValidator.AllEdgesHaveTwins(mesh));
    }

    [Fact]
    public void FaceCycles_Valid()
    {
        var positions = new Vec3[]
        {
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), new Vec3(0, 0, 1)
        };
        var indices = new (int, int, int)[] { (0, 2, 1), (0, 1, 3), (0, 3, 2), (1, 2, 3) };
        var mesh = new MeshBuilder().Build(positions, indices);
        Assert.True(MeshValidator.HasValidFaceCycles(mesh));
    }

    [Fact]
    public void HalfEdgeCount_Is3PerFace()
    {
        var positions = new Vec3[] { Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0) };
        var indices = new (int, int, int)[] { (0, 1, 2) };
        var mesh = new MeshBuilder().Build(positions, indices);
        Assert.Equal(3, mesh.HalfEdges.Count);
    }

    [Fact]
    public void MultipleTriangles_HalfEdgeCount()
    {
        var positions = new Vec3[]
        {
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), new Vec3(0, 0, 1)
        };
        var indices = new (int, int, int)[] { (0, 2, 1), (0, 1, 3), (0, 3, 2), (1, 2, 3) };
        var mesh = new MeshBuilder().Build(positions, indices);
        Assert.Equal(12, mesh.HalfEdges.Count); // 4 faces × 3
    }

    [Fact]
    public void Welding_NearVertices_Merged()
    {
        var positions = new Vec3[]
        {
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0),
            new Vec3(1e-11, 0, 0), new Vec3(1 + 1e-11, 0, 0), new Vec3(0, 1 + 1e-11, 0),
        };
        var indices = new (int, int, int)[] { (0, 1, 2), (3, 4, 5) };
        // With tight weld tolerance, should merge
        var mesh = new MeshBuilder(1e-8).Build(new List<Triangle3>
        {
            new Triangle3(positions[0], positions[1], positions[2]),
            new Triangle3(positions[3], positions[4], positions[5]),
        });
        Assert.True(mesh.Vertices.Count <= 4, $"Expected welded vertices <= 4, got {mesh.Vertices.Count}");
    }

    [Fact]
    public void NoWelding_LargeTolerance()
    {
        var tris = new List<Triangle3>
        {
            new Triangle3(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new Triangle3(new Vec3(0.5, 0, 0), new Vec3(1.5, 0, 0), new Vec3(0.5, 1, 0)),
        };
        var mesh = new MeshBuilder(1e-10).Build(tris);
        // Vertices at different positions should not be welded
        Assert.True(mesh.Vertices.Count >= 5);
    }

    [Fact]
    public void FaceEdge_PointsToFace()
    {
        var positions = new Vec3[] { Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0) };
        var indices = new (int, int, int)[] { (0, 1, 2) };
        var mesh = new MeshBuilder().Build(positions, indices);
        var face = mesh.Faces[0];
        Assert.Same(face, face.Edge.Face);
    }

    [Fact]
    public void VertexPositions_Preserved()
    {
        var positions = new Vec3[] { new Vec3(1, 2, 3), new Vec3(4, 5, 6), new Vec3(7, 8, 9) };
        var indices = new (int, int, int)[] { (0, 1, 2) };
        var mesh = new MeshBuilder().Build(positions, indices);
        var vertPositions = mesh.Vertices.Select(v => v.Position).ToList();
        foreach (var p in positions)
        {
            Assert.Contains(p, vertPositions);
        }
    }
}
