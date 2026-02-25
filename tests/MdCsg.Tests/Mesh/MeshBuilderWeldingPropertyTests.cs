using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Tests.Mesh;

/// <summary>Phase 6: MeshBuilder — vertex welding, twin linking, indexed build, tolerance behavior</summary>
public class MeshBuilderWeldingPropertyTests
{
    [Fact]
    public void Build_SingleTriangle_ThreeVertices()
    {
        var builder = new MeshBuilder();
        var tris = new[] { new Triangle3(Vec3.Zero, Vec3.UnitX, Vec3.UnitY) };
        var mesh = builder.Build(tris);
        Assert.Equal(3, mesh.Vertices.Count);
        Assert.Equal(1, mesh.Faces.Count);
    }

    [Fact]
    public void Build_SingleTriangle_ThreeHalfEdges()
    {
        var builder = new MeshBuilder();
        var tris = new[] { new Triangle3(Vec3.Zero, Vec3.UnitX, Vec3.UnitY) };
        var mesh = builder.Build(tris);
        Assert.Equal(3, mesh.HalfEdges.Count);
    }

    [Fact]
    public void Build_TwoSharedEdge_FourVertices()
    {
        var builder = new MeshBuilder();
        var tris = new[]
        {
            new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new Triangle3(new Vec3(1, 0, 0), new Vec3(1, 1, 0), new Vec3(0, 1, 0)),
        };
        var mesh = builder.Build(tris);
        Assert.Equal(4, mesh.Vertices.Count);
    }

    [Fact]
    public void Build_TwoSharedEdge_TwinsLinked()
    {
        var builder = new MeshBuilder();
        var tris = new[]
        {
            new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new Triangle3(new Vec3(1, 0, 0), new Vec3(1, 1, 0), new Vec3(0, 1, 0)),
        };
        var mesh = builder.Build(tris);
        int twinCount = mesh.HalfEdges.Count(he => he.Twin != null);
        Assert.True(twinCount >= 2, $"Should have at least 2 half-edges with twins, got {twinCount}");
    }

    [Fact]
    public void Build_WeldTolerance_MergesNearbyVertices()
    {
        var builder = new MeshBuilder(1e-6);
        var tris = new[]
        {
            new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new Triangle3(new Vec3(1.0000001, 0, 0), new Vec3(1, 1, 0), new Vec3(0.0000001, 1, 0)),
        };
        var mesh = builder.Build(tris);
        Assert.Equal(4, mesh.Vertices.Count); // Welded vertices
    }

    [Fact]
    public void Build_LargeWeldTolerance_DoesNotCollapse()
    {
        // Even with large tolerance, vertices that are far apart should not weld
        var builder = new MeshBuilder(0.1);
        var tris = new[]
        {
            new Triangle3(new Vec3(0, 0, 0), new Vec3(5, 0, 0), new Vec3(0, 5, 0)),
        };
        var mesh = builder.Build(tris);
        Assert.Equal(3, mesh.Vertices.Count);
    }

    [Fact]
    public void Build_ZeroTriangles_EmptyMesh()
    {
        var builder = new MeshBuilder();
        var mesh = builder.Build(Array.Empty<Triangle3>());
        Assert.Equal(0, mesh.Vertices.Count);
        Assert.Equal(0, mesh.Faces.Count);
    }

    [Fact]
    public void BuildIndexed_SingleTriangle_Works()
    {
        var builder = new MeshBuilder();
        var positions = new Vec3[] { Vec3.Zero, Vec3.UnitX, Vec3.UnitY };
        var indices = new (int, int, int)[] { (0, 1, 2) };
        var mesh = builder.Build(positions, indices);
        Assert.Equal(3, mesh.Vertices.Count);
        Assert.Equal(1, mesh.Faces.Count);
    }

    [Fact]
    public void BuildIndexed_SharedVertex_NoWelding()
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
    public void BuildIndexed_TwinsLinked()
    {
        var builder = new MeshBuilder();
        var positions = new Vec3[]
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 1, 0), new(1, 1, 0)
        };
        var indices = new (int, int, int)[] { (0, 1, 2), (1, 3, 2) };
        var mesh = builder.Build(positions, indices);
        int twinCount = mesh.HalfEdges.Count(he => he.Twin != null);
        Assert.True(twinCount >= 2);
    }

    [Fact]
    public void Build_FaceHasCorrectVertices()
    {
        var builder = new MeshBuilder();
        var a = new Vec3(1, 2, 3);
        var b = new Vec3(4, 5, 6);
        var c = new Vec3(7, 8, 9);
        var mesh = builder.Build(new[] { new Triangle3(a, b, c) });
        var face = mesh.Faces[0];
        face.GetTrianglePositions(out var va, out var vb, out var vc);
        // Vertices may be rotated but should contain all three positions
        var expected = new HashSet<Vec3> { a, b, c };
        var actual = new HashSet<Vec3> { va, vb, vc };
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Build_HalfEdgeCirculation_Correct()
    {
        var builder = new MeshBuilder();
        var mesh = builder.Build(new[] { new Triangle3(Vec3.Zero, Vec3.UnitX, Vec3.UnitY) });
        var face = mesh.Faces[0];
        var start = face.Edge;
        int count = 0;
        var current = start;
        do
        {
            count++;
            current = current.Next;
        } while (current != start && count < 10);
        Assert.Equal(3, count);
    }

    [Fact]
    public void Build_ClosedTetrahedron_AllTwinsLinked()
    {
        var builder = new MeshBuilder();
        var v0 = new Vec3(0, 0, 0);
        var v1 = new Vec3(1, 0, 0);
        var v2 = new Vec3(0.5, 0.866, 0);
        var v3 = new Vec3(0.5, 0.289, 0.816);
        var tris = new[]
        {
            new Triangle3(v0, v2, v1), // bottom
            new Triangle3(v0, v1, v3), // front
            new Triangle3(v1, v2, v3), // right
            new Triangle3(v2, v0, v3), // left
        };
        var mesh = builder.Build(tris);
        int unlinked = mesh.HalfEdges.Count(he => he.Twin == null);
        Assert.Equal(0, unlinked);
    }
}
