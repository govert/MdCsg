using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Tests.Mesh;

/// <summary>Phase 6: MeshBuilder — indexed build path, twin linking, no welding</summary>
public class MeshBuilderIndexedBuildTests
{
    [Fact]
    public void IndexedBuild_SingleTriangle()
    {
        var positions = new List<Vec3>
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 1, 0),
        };
        var indices = new List<(int, int, int)> { (0, 1, 2) };
        var builder = new MeshBuilder();
        var mesh = builder.Build(positions, indices);
        Assert.Equal(3, mesh.Vertices.Count);
        Assert.Single(mesh.Faces);
    }

    [Fact]
    public void IndexedBuild_TwoTriangles_SharedVertices()
    {
        var positions = new List<Vec3>
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 1, 0), new(1, 1, 0),
        };
        var indices = new List<(int, int, int)> { (0, 1, 2), (1, 3, 2) };
        var builder = new MeshBuilder();
        var mesh = builder.Build(positions, indices);
        Assert.Equal(4, mesh.Vertices.Count);
        Assert.Equal(2, mesh.Faces.Count);
    }

    [Fact]
    public void IndexedBuild_TwinLinking()
    {
        var positions = new List<Vec3>
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 1, 0), new(1, 1, 0),
        };
        var indices = new List<(int, int, int)> { (0, 1, 2), (1, 3, 2) };
        var builder = new MeshBuilder();
        var mesh = builder.Build(positions, indices);

        // Check that some twins are linked
        int twinCount = mesh.HalfEdges.Count(he => he.Twin != null);
        Assert.True(twinCount > 0, "Should have some twin-linked half-edges");
    }

    [Fact]
    public void IndexedBuild_Tetrahedron()
    {
        var positions = new List<Vec3>
        {
            new(0, 0, 0), new(1, 0, 0), new(0.5, 0.866, 0), new(0.5, 0.289, 0.816),
        };
        var indices = new List<(int, int, int)>
        {
            (0, 2, 1), (0, 1, 3), (1, 2, 3), (0, 3, 2),
        };
        var builder = new MeshBuilder();
        var mesh = builder.Build(positions, indices);
        Assert.Equal(4, mesh.Vertices.Count);
        Assert.Equal(4, mesh.Faces.Count);
        Assert.Equal(12, mesh.HalfEdges.Count);
    }

    [Fact]
    public void TriangleBuild_WeldsNearbyVertices()
    {
        var triangles = new List<Triangle3>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new(new Vec3(1, 0, 0), new Vec3(1, 1, 0), new Vec3(0, 1, 0)),
        };
        var builder = new MeshBuilder(1e-8);
        var mesh = builder.Build(triangles);
        // Shared vertices should be welded
        Assert.True(mesh.Vertices.Count <= 4, $"Expected <= 4 vertices, got {mesh.Vertices.Count}");
        Assert.Equal(2, mesh.Faces.Count);
    }

    [Fact]
    public void TriangleBuild_NoWeldingWithLargeSeparation()
    {
        var triangles = new List<Triangle3>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new(new Vec3(10, 0, 0), new Vec3(11, 0, 0), new Vec3(10, 1, 0)),
        };
        var builder = new MeshBuilder(1e-8);
        var mesh = builder.Build(triangles);
        Assert.Equal(6, mesh.Vertices.Count);
    }

    [Fact]
    public void IndexedBuild_FaceCycles_Valid()
    {
        var positions = new List<Vec3>
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 1, 0),
        };
        var indices = new List<(int, int, int)> { (0, 1, 2) };
        var builder = new MeshBuilder();
        var mesh = builder.Build(positions, indices);

        // Check face cycle: 3 half-edges
        var face = mesh.Faces[0];
        var start = face.Edge;
        var current = start;
        int count = 0;
        do
        {
            count++;
            current = current.Next;
        } while (current != start && count < 10);
        Assert.Equal(3, count);
    }

    [Fact]
    public void IndexedBuild_PrevNextConsistency()
    {
        var positions = new List<Vec3>
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 1, 0),
        };
        var indices = new List<(int, int, int)> { (0, 1, 2) };
        var builder = new MeshBuilder();
        var mesh = builder.Build(positions, indices);

        foreach (var he in mesh.HalfEdges)
        {
            Assert.Equal(he, he.Next.Prev);
            Assert.Equal(he, he.Prev.Next);
        }
    }

    [Fact]
    public void DefaultTolerance_IsSmall()
    {
        var builder = new MeshBuilder();
        // Default builder should work without error
        var mesh = builder.Build(new List<Triangle3>
        {
            new(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
        });
        Assert.Single(mesh.Faces);
    }

    [Fact]
    public void IndexedBuild_OutgoingEdges_Set()
    {
        var positions = new List<Vec3>
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 1, 0),
        };
        var indices = new List<(int, int, int)> { (0, 1, 2) };
        var builder = new MeshBuilder();
        var mesh = builder.Build(positions, indices);

        foreach (var v in mesh.Vertices)
            Assert.NotNull(v.OutgoingEdge);
    }
}
