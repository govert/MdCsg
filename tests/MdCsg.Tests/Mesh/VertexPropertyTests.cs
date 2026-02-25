using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Tests.Mesh;

/// <summary>Phase 6: Vertex class — Id, Position, OutgoingEdge, state management</summary>
public class VertexPropertyTests
{
    [Fact]
    public void Constructor_IdPreserved()
    {
        var v = new Vertex(42, new Vec3(1, 2, 3));
        Assert.Equal(42, v.Id);
    }

    [Fact]
    public void Constructor_PositionPreserved()
    {
        var pos = new Vec3(1, 2, 3);
        var v = new Vertex(0, pos);
        Assert.Equal(pos, v.Position);
    }

    [Fact]
    public void OutgoingEdge_InitiallyNull()
    {
        var v = new Vertex(0, Vec3.Zero);
        Assert.Null(v.OutgoingEdge);
    }

    [Fact]
    public void Position_IsMutable()
    {
        var v = new Vertex(0, Vec3.Zero);
        v.Position = new Vec3(5, 6, 7);
        Assert.Equal(5, v.Position.X, 1e-14);
        Assert.Equal(6, v.Position.Y, 1e-14);
        Assert.Equal(7, v.Position.Z, 1e-14);
    }

    [Fact]
    public void OutgoingEdge_CanBeAssigned()
    {
        var v = new Vertex(0, Vec3.Zero);
        var he = new HalfEdge(0);
        v.OutgoingEdge = he;
        Assert.Same(he, v.OutgoingEdge);
    }

    [Fact]
    public void OutgoingEdge_CanBeReassigned()
    {
        var v = new Vertex(0, Vec3.Zero);
        var he1 = new HalfEdge(0);
        var he2 = new HalfEdge(1);
        v.OutgoingEdge = he1;
        v.OutgoingEdge = he2;
        Assert.Same(he2, v.OutgoingEdge);
    }

    [Fact]
    public void TwoVertices_SameIdDifferentPosition()
    {
        var v1 = new Vertex(0, new Vec3(1, 0, 0));
        var v2 = new Vertex(0, new Vec3(0, 1, 0));
        Assert.NotEqual(v1.Position, v2.Position);
        Assert.Equal(v1.Id, v2.Id);
    }

    [Fact]
    public void ZeroId_IsValid()
    {
        var v = new Vertex(0, Vec3.Zero);
        Assert.Equal(0, v.Id);
    }

    [Fact]
    public void NegativeId_IsAllowed()
    {
        var v = new Vertex(-1, Vec3.Zero);
        Assert.Equal(-1, v.Id);
    }

    [Fact]
    public void MeshBuilder_VerticesHavePositions()
    {
        var builder = new MeshBuilder();
        var mesh = builder.Build(new[]
        {
            new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0))
        });
        Assert.Equal(3, mesh.Vertices.Count);
        foreach (var v in mesh.Vertices)
        {
            Assert.NotNull(v);
            Assert.True(v.Id >= 0);
        }
    }

    [Fact]
    public void MeshBuilder_VerticesHaveOutgoingEdges()
    {
        var builder = new MeshBuilder();
        var mesh = builder.Build(new[]
        {
            new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0))
        });
        foreach (var v in mesh.Vertices)
        {
            Assert.NotNull(v.OutgoingEdge);
        }
    }

    [Fact]
    public void Cube_VerticesHaveUniqueIds()
    {
        var mesh = TestHelpers.MeshFactory.CreateCube().Mesh;
        var ids = new HashSet<int>();
        foreach (var v in mesh.Vertices)
            Assert.True(ids.Add(v.Id), $"Duplicate vertex Id: {v.Id}");
    }
}
