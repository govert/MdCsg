using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Tests.Mesh;

/// <summary>Phase 6: MeshBuilder edge cases and welding behavior</summary>
public class MeshBuilderEdgeCaseTests
{
    [Fact]
    public void Build_SingleTriangle()
    {
        var tris = new Triangle3[]
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0))
        };
        var builder = new MeshBuilder(1e-8);
        var mesh = builder.Build(tris);
        Assert.Equal(1, mesh.Faces.Count);
        Assert.Equal(3, mesh.Vertices.Count);
        Assert.Equal(3, mesh.HalfEdges.Count);
    }

    [Fact]
    public void Build_TwoTriangles_SharedEdge()
    {
        var tris = new Triangle3[]
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new(new Vec3(1, 0, 0), new Vec3(1, 1, 0), new Vec3(0, 1, 0))
        };
        var builder = new MeshBuilder(1e-8);
        var mesh = builder.Build(tris);
        Assert.Equal(4, mesh.Vertices.Count);
        Assert.Equal(2, mesh.Faces.Count);
        Assert.Equal(6, mesh.HalfEdges.Count);
    }

    [Fact]
    public void Build_TwoTriangles_TwinLinking()
    {
        var tris = new Triangle3[]
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new(new Vec3(1, 0, 0), new Vec3(1, 1, 0), new Vec3(0, 1, 0))
        };
        var builder = new MeshBuilder(1e-8);
        var mesh = builder.Build(tris);
        bool hasTwin = mesh.HalfEdges.Any(he => he.Twin != null);
        Assert.True(hasTwin, "Shared edge should have twin linkage");
    }

    [Fact]
    public void Build_NearbyVertices_Welded()
    {
        double eps = 1e-12;
        var tris = new Triangle3[]
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new(new Vec3(1 + eps, 0 + eps, 0), new Vec3(1, 1, 0), new Vec3(0 + eps, 1 + eps, 0))
        };
        var builder = new MeshBuilder(1e-8);
        var mesh = builder.Build(tris);
        Assert.Equal(4, mesh.Vertices.Count); // 2 shared vertices welded
    }

    [Fact]
    public void Build_FarVertices_NotWelded()
    {
        var tris = new Triangle3[]
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new(new Vec3(1.1, 0, 0), new Vec3(1, 1, 0), new Vec3(0, 1.1, 0))
        };
        var builder = new MeshBuilder(1e-8);
        var mesh = builder.Build(tris);
        Assert.True(mesh.Vertices.Count > 4); // not welded
    }

    [Fact]
    public void Build_Indexed_CorrectVertexCount()
    {
        var positions = new Vec3[]
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 1, 0), new(1, 1, 0)
        };
        var indices = new (int, int, int)[]
        {
            (0, 1, 2), (1, 3, 2)
        };
        var builder = new MeshBuilder(1e-8);
        var mesh = builder.Build(positions, indices);
        Assert.Equal(4, mesh.Vertices.Count);
        Assert.Equal(2, mesh.Faces.Count);
    }

    [Fact]
    public void Build_Indexed_TwinsLinked()
    {
        var positions = new Vec3[]
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 1, 0), new(1, 1, 0)
        };
        var indices = new (int, int, int)[]
        {
            (0, 1, 2), (1, 3, 2)
        };
        var builder = new MeshBuilder(1e-8);
        var mesh = builder.Build(positions, indices);
        int twinCount = mesh.HalfEdges.Count(he => he.Twin != null);
        Assert.True(twinCount >= 2, "Shared edge should have 2 half-edges with twins");
    }

    [Fact]
    public void Build_ThreeTriangleFan()
    {
        var tris = new Triangle3[]
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0.5, 0.5, 0)),
            new(new Vec3(0, 0, 0), new Vec3(0.5, 0.5, 0), new Vec3(0, 1, 0)),
            new(new Vec3(0, 0, 0), new Vec3(0, 1, 0), new Vec3(-0.5, 0.5, 0)),
        };
        var builder = new MeshBuilder(1e-8);
        var mesh = builder.Build(tris);
        Assert.Equal(3, mesh.Faces.Count);
        Assert.Equal(5, mesh.Vertices.Count); // 5 unique vertices
    }

    [Fact]
    public void Build_DefaultTolerance()
    {
        var tris = new Triangle3[]
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0))
        };
        var builder = new MeshBuilder(); // default tolerance
        var mesh = builder.Build(tris);
        Assert.Equal(1, mesh.Faces.Count);
    }

    [Fact]
    public void Build_FaceCycleValid()
    {
        var tris = new Triangle3[]
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0))
        };
        var builder = new MeshBuilder(1e-8);
        var mesh = builder.Build(tris);
        foreach (var face in mesh.Faces)
        {
            var edge = face.Edge;
            int count = 0;
            var current = edge;
            do
            {
                count++;
                current = current.Next;
            } while (current != edge && count < 10);
            Assert.Equal(3, count);
        }
    }

    [Fact]
    public void Build_VertexOutgoingEdge_SetCorrectly()
    {
        var tris = new Triangle3[]
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0))
        };
        var builder = new MeshBuilder(1e-8);
        var mesh = builder.Build(tris);
        foreach (var v in mesh.Vertices)
        {
            Assert.NotNull(v.OutgoingEdge);
            Assert.Equal(v.Id, v.OutgoingEdge.Origin.Id);
        }
    }

    [Fact]
    public void Build_HalfEdge_NextPrevConsistent()
    {
        var tris = new Triangle3[]
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new(new Vec3(1, 0, 0), new Vec3(1, 1, 0), new Vec3(0, 1, 0))
        };
        var builder = new MeshBuilder(1e-8);
        var mesh = builder.Build(tris);
        foreach (var he in mesh.HalfEdges)
        {
            Assert.Equal(he, he.Next.Prev);
        }
    }
}
