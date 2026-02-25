using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Tests.Mesh;

/// <summary>Phase 6: MeshBuilder — vertex welding, twin linking, Build(triangles), Build(indexed)</summary>
public class MeshBuilderWeldPropertyTests
{
    [Fact]
    public void Build_SingleTriangle_3Vertices3HalfEdges1Face()
    {
        var builder = new MeshBuilder();
        var mesh = builder.Build(new[]
        {
            new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0))
        });
        Assert.Equal(3, mesh.Vertices.Count);
        Assert.Equal(3, mesh.HalfEdges.Count);
        Assert.Single(mesh.Faces);
    }

    [Fact]
    public void Build_TwoAdjacentTriangles_WeldsSharedVertices()
    {
        var builder = new MeshBuilder();
        var mesh = builder.Build(new[]
        {
            new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new Triangle3(new Vec3(1, 0, 0), new Vec3(0, 0, 0), new Vec3(0, -1, 0))
        });
        // Should weld 2 shared vertices: (0,0,0) and (1,0,0)
        Assert.Equal(4, mesh.Vertices.Count);
        Assert.Equal(2, mesh.Faces.Count);
    }

    [Fact]
    public void Build_TwoAdjacentTriangles_TwinsLinked()
    {
        var builder = new MeshBuilder();
        var mesh = builder.Build(new[]
        {
            new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new Triangle3(new Vec3(1, 0, 0), new Vec3(0, 0, 0), new Vec3(0, -1, 0))
        });
        int twinCount = 0;
        foreach (var he in mesh.HalfEdges)
            if (he.Twin != null) twinCount++;
        Assert.True(twinCount > 0, "Should have linked twins for shared edge");
    }

    [Fact]
    public void Build_WeldTolerance_MergesNearbyVertices()
    {
        var builder = new MeshBuilder(1e-6);
        var mesh = builder.Build(new[]
        {
            new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new Triangle3(new Vec3(1.0000001e-7, 0, 0), new Vec3(1.0000001, 0, 0), new Vec3(0, 1.0000001e-7, 0))
        });
        // With large-ish tolerance, nearby vertices get welded
        Assert.True(mesh.Vertices.Count < 6);
    }

    [Fact]
    public void Build_Indexed_SameAsSoup()
    {
        var builder = new MeshBuilder();
        var positions = new Vec3[]
        {
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), new Vec3(1, 1, 0)
        };
        var indices = new (int, int, int)[] { (0, 1, 2), (1, 3, 2) };
        var mesh = builder.Build(positions, indices);
        Assert.Equal(4, mesh.Vertices.Count);
        Assert.Equal(2, mesh.Faces.Count);
        Assert.Equal(6, mesh.HalfEdges.Count);
    }

    [Fact]
    public void Build_Indexed_TwinsLinked()
    {
        var builder = new MeshBuilder();
        var positions = new Vec3[]
        {
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), new Vec3(1, 1, 0)
        };
        var indices = new (int, int, int)[] { (0, 1, 2), (1, 3, 2) };
        var mesh = builder.Build(positions, indices);
        int twinCount = 0;
        foreach (var he in mesh.HalfEdges)
            if (he.Twin != null) twinCount++;
        Assert.True(twinCount >= 2, "Shared edge should have twins");
    }

    [Fact]
    public void Build_Empty_EmptyMesh()
    {
        var builder = new MeshBuilder();
        var mesh = builder.Build(Array.Empty<Triangle3>());
        Assert.Equal(0, mesh.Vertices.Count);
        Assert.Equal(0, mesh.Faces.Count);
        Assert.Equal(0, mesh.HalfEdges.Count);
    }

    [Fact]
    public void Build_AllFacesHaveEdge()
    {
        var builder = new MeshBuilder();
        var mesh = builder.Build(new[]
        {
            new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new Triangle3(new Vec3(0, 0, 0), new Vec3(0, 1, 0), new Vec3(0, 0, 1))
        });
        foreach (var face in mesh.Faces)
        {
            Assert.NotNull(face.Edge);
        }
    }

    [Fact]
    public void Build_AllHalfEdgesHaveTarget()
    {
        var builder = new MeshBuilder();
        var mesh = builder.Build(new[]
        {
            new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0))
        });
        foreach (var he in mesh.HalfEdges)
        {
            Assert.NotNull(he.Target);
        }
    }

    [Fact]
    public void Build_AllHalfEdgesHaveNextAndPrev()
    {
        var builder = new MeshBuilder();
        var mesh = builder.Build(new[]
        {
            new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0))
        });
        foreach (var he in mesh.HalfEdges)
        {
            Assert.NotNull(he.Next);
            Assert.NotNull(he.Prev);
        }
    }

    [Fact]
    public void Build_NextPrev_AreInverse()
    {
        var builder = new MeshBuilder();
        var mesh = builder.Build(new[]
        {
            new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0))
        });
        foreach (var he in mesh.HalfEdges)
        {
            Assert.Equal(he, he.Next.Prev);
            Assert.Equal(he, he.Prev.Next);
        }
    }

    [Fact]
    public void Build_TwinSymmetry()
    {
        var builder = new MeshBuilder();
        var mesh = builder.Build(new[]
        {
            new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new Triangle3(new Vec3(1, 0, 0), new Vec3(0, 0, 0), new Vec3(0, -1, 0))
        });
        foreach (var he in mesh.HalfEdges)
        {
            if (he.Twin != null)
                Assert.Equal(he, he.Twin.Twin);
        }
    }

    [Fact]
    public void Build_ThreeTriangles_CorrectVertexCount()
    {
        var builder = new MeshBuilder();
        var mesh = builder.Build(new[]
        {
            new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new Triangle3(new Vec3(1, 0, 0), new Vec3(1, 1, 0), new Vec3(0, 1, 0)),
            new Triangle3(new Vec3(1, 0, 0), new Vec3(2, 0, 0), new Vec3(1, 1, 0))
        });
        Assert.Equal(5, mesh.Vertices.Count);
        Assert.Equal(3, mesh.Faces.Count);
    }
}
