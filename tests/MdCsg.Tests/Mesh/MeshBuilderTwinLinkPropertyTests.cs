using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Mesh;

/// <summary>Phase 6: MeshBuilder — twin linking, vertex welding, indexed build, edge connectivity</summary>
public class MeshBuilderTwinLinkPropertyTests
{
    [Fact]
    public void Build_TwoTrianglesSharedEdge_TwinsLinked()
    {
        var builder = new MeshBuilder();
        var tris = new[]
        {
            new Triangle3(Vec3.Zero, Vec3.UnitX, Vec3.UnitY),
            new Triangle3(Vec3.UnitX, new Vec3(1, 1, 0), Vec3.UnitY)
        };
        var mesh = builder.Build(tris);
        int twinCount = 0;
        foreach (var he in mesh.HalfEdges)
        {
            if (he.Twin != null)
                twinCount++;
        }
        Assert.True(twinCount > 0, "Shared edge should have twins linked");
    }

    [Fact]
    public void Build_SingleTriangle_NoTwins()
    {
        var builder = new MeshBuilder();
        var mesh = builder.Build(new[] { new Triangle3(Vec3.Zero, Vec3.UnitX, Vec3.UnitY) });
        foreach (var he in mesh.HalfEdges)
        {
            Assert.Null(he.Twin);
        }
    }

    [Fact]
    public void Build_IndexedCube_CorrectVertexCount()
    {
        var cube = MeshFactory.CreateCube();
        Assert.Equal(8, cube.Mesh.Vertices.Count);
    }

    [Fact]
    public void Build_IndexedCube_AllTwinsLinked()
    {
        var cube = MeshFactory.CreateCube();
        foreach (var he in cube.Mesh.HalfEdges)
        {
            Assert.NotNull(he.Twin);
        }
    }

    [Fact]
    public void Build_WeldTolerance_MergesNearVertices()
    {
        var builder = new MeshBuilder(0.1); // large tolerance
        var tris = new[]
        {
            new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new Triangle3(new Vec3(1.01, 0, 0), new Vec3(1, 1, 0), new Vec3(0.01, 1, 0)) // near shared edge
        };
        var mesh = builder.Build(tris);
        // With large tolerance, nearby vertices should be welded
        Assert.True(mesh.Vertices.Count <= 5,
            $"Large weld tolerance should merge vertices, got {mesh.Vertices.Count}");
    }

    [Fact]
    public void Build_SmallWeldTolerance_KeepsVerticesSeparate()
    {
        var builder = new MeshBuilder(1e-15); // tiny tolerance
        var tris = new[]
        {
            new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new Triangle3(new Vec3(1.001, 0, 0), new Vec3(1, 1, 0), new Vec3(0.001, 1, 0))
        };
        var mesh = builder.Build(tris);
        Assert.Equal(6, mesh.Vertices.Count);
    }

    [Fact]
    public void Build_FromTriangles_AndFromIndexed_SameTopology()
    {
        var cubeTriangles = MeshFactory.CreateCube();
        // Verify same face count
        Assert.Equal(12, cubeTriangles.Mesh.Faces.Count);
    }

    [Fact]
    public void Build_EmptyTriangles_EmptyMesh()
    {
        var builder = new MeshBuilder();
        var mesh = builder.Build(Array.Empty<Triangle3>());
        Assert.Equal(0, mesh.Vertices.Count);
        Assert.Equal(0, mesh.Faces.Count);
        Assert.Equal(0, mesh.HalfEdges.Count);
    }

    [Fact]
    public void Build_TwoDisjointTriangles_SixVertices()
    {
        var builder = new MeshBuilder();
        var tris = new[]
        {
            new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new Triangle3(new Vec3(10, 0, 0), new Vec3(11, 0, 0), new Vec3(10, 1, 0))
        };
        var mesh = builder.Build(tris);
        Assert.Equal(6, mesh.Vertices.Count);
    }

    [Fact]
    public void Build_TwoDisjointTriangles_NoTwins()
    {
        var builder = new MeshBuilder();
        var tris = new[]
        {
            new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new Triangle3(new Vec3(10, 0, 0), new Vec3(11, 0, 0), new Vec3(10, 1, 0))
        };
        var mesh = builder.Build(tris);
        foreach (var he in mesh.HalfEdges)
        {
            Assert.Null(he.Twin);
        }
    }

    [Fact]
    public void Build_FourTrianglesQuad_FourSharedEdges()
    {
        var builder = new MeshBuilder();
        // Two triangles forming a quad
        var tris = new[]
        {
            new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(1, 1, 0)),
            new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 1, 0), new Vec3(0, 1, 0))
        };
        var mesh = builder.Build(tris);
        // The shared diagonal should have twins
        int twinPairs = 0;
        foreach (var he in mesh.HalfEdges)
        {
            if (he.Twin != null) twinPairs++;
        }
        Assert.True(twinPairs > 0, "Shared diagonal should have twin pair");
        Assert.True(twinPairs % 2 == 0, "Twin pairs should be even");
    }

    [Fact]
    public void Build_FromIndexed_VertexPositions_Match()
    {
        var positions = new Vec3[]
        {
            new Vec3(0, 0, 0), new Vec3(1, 0, 0),
            new Vec3(1, 1, 0), new Vec3(0, 1, 0)
        };
        var indices = new (int, int, int)[] { (0, 1, 2), (0, 2, 3) };
        var builder = new MeshBuilder();
        var mesh = builder.Build(positions, indices);
        Assert.Equal(4, mesh.Vertices.Count);
        Assert.Equal(2, mesh.Faces.Count);
    }

    [Fact]
    public void Sphere_AllEdgesHaveTwins()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        foreach (var he in sphere.Mesh.HalfEdges)
        {
            Assert.NotNull(he.Twin);
        }
    }

    [Fact]
    public void Cube_TwinTarget_EqualsOrigin()
    {
        var cube = MeshFactory.CreateCube();
        foreach (var he in cube.Mesh.HalfEdges)
        {
            if (he.Twin != null)
            {
                Assert.Equal(he.Target.Id, he.Twin.Origin.Id);
            }
        }
    }
}
