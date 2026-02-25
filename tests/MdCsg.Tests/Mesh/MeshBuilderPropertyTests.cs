using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Tests.Mesh;

/// <summary>Phase 6: MeshBuilder — Build from triangles, vertex welding, twin linking, indexed build</summary>
public class MeshBuilderPropertyTests
{
    [Fact]
    public void Build_SingleTriangle_ThreeVertices()
    {
        var builder = new MeshBuilder();
        var tris = new[] { new Triangle3(Vec3.Zero, Vec3.UnitX, Vec3.UnitY) };
        var mesh = builder.Build(tris);
        Assert.Equal(3, mesh.Vertices.Count);
        Assert.Single(mesh.Faces);
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
    public void Build_TwoTrianglesSharedEdge_WeldsVertices()
    {
        var builder = new MeshBuilder();
        var tris = new[]
        {
            new Triangle3(new Vec3(0,0,0), new Vec3(1,0,0), new Vec3(0,1,0)),
            new Triangle3(new Vec3(1,0,0), new Vec3(0,0,0), new Vec3(0,0,1)),
        };
        var mesh = builder.Build(tris);
        // Shared edge has 2 common vertices, plus 2 unique = 4 total
        Assert.Equal(4, mesh.Vertices.Count);
        Assert.Equal(2, mesh.Faces.Count);
    }

    [Fact]
    public void Build_TwoTrianglesSharedEdge_TwinsLinked()
    {
        var builder = new MeshBuilder();
        var tris = new[]
        {
            new Triangle3(new Vec3(0,0,0), new Vec3(1,0,0), new Vec3(0,1,0)),
            new Triangle3(new Vec3(1,0,0), new Vec3(0,0,0), new Vec3(0,0,1)),
        };
        var mesh = builder.Build(tris);
        int twinCount = mesh.HalfEdges.Count(he => he.Twin != null);
        Assert.True(twinCount >= 2); // At least the shared edge pair
    }

    [Fact]
    public void Build_CustomWeldTolerance_MergesClose()
    {
        var builder = new MeshBuilder(0.01);
        var tris = new[]
        {
            new Triangle3(new Vec3(0,0,0), new Vec3(1,0,0), new Vec3(0,1,0)),
            new Triangle3(new Vec3(1.005,0,0), new Vec3(0,0,0.005), new Vec3(0,1.005,0)),
        };
        var mesh = builder.Build(tris);
        // With tolerance 0.01, vertices within 0.01 should merge
        Assert.True(mesh.Vertices.Count < 6);
    }

    [Fact]
    public void Build_Indexed_CreatesCorrectMesh()
    {
        var builder = new MeshBuilder();
        var positions = new Vec3[] { Vec3.Zero, Vec3.UnitX, Vec3.UnitY, Vec3.UnitZ };
        var indices = new (int, int, int)[] { (0, 1, 2), (0, 2, 3) };
        var mesh = builder.Build(positions, indices);
        Assert.Equal(4, mesh.Vertices.Count);
        Assert.Equal(2, mesh.Faces.Count);
    }

    [Fact]
    public void Build_Indexed_TwinsLinked()
    {
        var builder = new MeshBuilder();
        var positions = new Vec3[] { Vec3.Zero, Vec3.UnitX, Vec3.UnitY, Vec3.UnitZ };
        var indices = new (int, int, int)[] { (0, 1, 2), (2, 1, 3) };
        var mesh = builder.Build(positions, indices);
        int twinCount = mesh.HalfEdges.Count(he => he.Twin != null);
        Assert.True(twinCount >= 2);
    }

    [Fact]
    public void Build_NoTriangles_EmptyMesh()
    {
        var builder = new MeshBuilder();
        var mesh = builder.Build(Array.Empty<Triangle3>());
        Assert.Empty(mesh.Vertices);
        Assert.Empty(mesh.Faces);
        Assert.Empty(mesh.HalfEdges);
    }

    [Fact]
    public void Build_DisjointTriangles_NoTwins()
    {
        var builder = new MeshBuilder();
        var tris = new[]
        {
            new Triangle3(new Vec3(0,0,0), new Vec3(1,0,0), new Vec3(0,1,0)),
            new Triangle3(new Vec3(10,10,10), new Vec3(11,10,10), new Vec3(10,11,10)),
        };
        var mesh = builder.Build(tris);
        Assert.Equal(6, mesh.Vertices.Count);
        int twinCount = mesh.HalfEdges.Count(he => he.Twin != null);
        Assert.Equal(0, twinCount);
    }

    [Fact]
    public void Build_CubeMesh_12Faces()
    {
        var builder = new MeshBuilder();
        // Build cube from 12 triangles (2 per face, 6 faces)
        var v = new Vec3[]
        {
            new(-1,-1,-1), new(1,-1,-1), new(1,1,-1), new(-1,1,-1),
            new(-1,-1,1),  new(1,-1,1),  new(1,1,1),  new(-1,1,1),
        };
        var tris = new Triangle3[]
        {
            // Front
            new(v[0], v[1], v[2]), new(v[0], v[2], v[3]),
            // Back
            new(v[5], v[4], v[7]), new(v[5], v[7], v[6]),
            // Top
            new(v[3], v[2], v[6]), new(v[3], v[6], v[7]),
            // Bottom
            new(v[4], v[5], v[1]), new(v[4], v[1], v[0]),
            // Right
            new(v[1], v[5], v[6]), new(v[1], v[6], v[2]),
            // Left
            new(v[4], v[0], v[3]), new(v[4], v[3], v[7]),
        };
        var mesh = builder.Build(tris);
        Assert.Equal(8, mesh.Vertices.Count);
        Assert.Equal(12, mesh.Faces.Count);
    }

    [Fact]
    public void Build_FaceEdgesFormCycle()
    {
        var builder = new MeshBuilder();
        var tris = new[] { new Triangle3(Vec3.Zero, Vec3.UnitX, Vec3.UnitY) };
        var mesh = builder.Build(tris);
        var face = mesh.Faces[0];
        var start = face.Edge;
        var current = start.Next;
        int count = 1;
        while (current != start && count < 10)
        {
            current = current.Next;
            count++;
        }
        Assert.Equal(3, count);
    }

    [Fact]
    public void Build_DefaultWeldTolerance_IsSmall()
    {
        var builder = new MeshBuilder();
        var tris = new[]
        {
            new Triangle3(new Vec3(0,0,0), new Vec3(1,0,0), new Vec3(0,1,0)),
            new Triangle3(new Vec3(0,0,0), new Vec3(0,1,0), new Vec3(0,0,1)),
        };
        var mesh = builder.Build(tris);
        // Default tolerance 1e-10 should weld exact duplicates
        Assert.Equal(4, mesh.Vertices.Count);
    }
}
