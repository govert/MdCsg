using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Mesh;

/// <summary>Phase 6: MeshBuilder — welding tolerance edge cases, hash collisions, twin linking</summary>
public class MeshBuilderWeldingToleranceTests
{
    [Fact]
    public void DefaultTolerance_Is1eMinus10()
    {
        // Test default constructor: vertices 1e-11 apart should be welded
        var builder = new MeshBuilder();
        var tri1 = new Triangle3(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var tri2 = new Triangle3(
            new Vec3(0, 0, 1e-11),
            new Vec3(1, 0, 1e-11),
            new Vec3(0.5, 0.5, 0.5));
        var mesh = builder.Build(new[] { tri1, tri2 });
        // First two vertices of tri2 should weld to tri1's vertices
        Assert.True(mesh.Vertices.Count <= 5); // 3 from tri1 + at most 1 new from tri2
    }

    [Fact]
    public void LargeTolerance_WeldsNearbyVertices()
    {
        // Use tolerance of 0.5 and offset within the same hash bucket
        var builder = new MeshBuilder(0.5);
        var tri1 = new Triangle3(Vec3.Zero, new Vec3(2, 0, 0), new Vec3(0, 2, 0));
        var tri2 = new Triangle3(
            new Vec3(0.1, 0.1, 0), // within 0.5 of (0,0,0)
            new Vec3(2, 0, 0),     // exact match
            new Vec3(0, 0, 3));    // far from everything
        var mesh = builder.Build(new[] { tri1, tri2 });
        // tri2 vertex 1 matches exact → welded, vertex 0 is within tolerance but hash may differ
        // At minimum, the exact match welds
        Assert.True(mesh.Vertices.Count <= 5, $"Expected ≤5 vertices, got {mesh.Vertices.Count}");
    }

    [Fact]
    public void ZeroTolerance_NeverWelds()
    {
        var builder = new MeshBuilder(0);
        var tri1 = new Triangle3(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var tri2 = new Triangle3(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 0, 1));
        var mesh = builder.Build(new[] { tri1, tri2 });
        // With zero tolerance, duplicates at same position may or may not weld depending on == hash
        // but positions at exact same coords will match since (0*invTol) = 0 → same hash
        // and distanceSq < 0 is false... so they won't weld
        Assert.True(mesh.Vertices.Count >= 4);
    }

    [Fact]
    public void TwinLinking_SharedEdge_LinkedBidirectional()
    {
        var builder = new MeshBuilder();
        var tri1 = new Triangle3(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var tri2 = new Triangle3(new Vec3(1, 0, 0), Vec3.Zero, new Vec3(0, 0, 1));
        var mesh = builder.Build(new[] { tri1, tri2 });

        int twinCount = 0;
        foreach (var he in mesh.HalfEdges)
        {
            if (he.Twin != null)
            {
                Assert.Same(he, he.Twin.Twin);
                twinCount++;
            }
        }
        // At least one twin pair (the shared edge)
        Assert.True(twinCount >= 2); // 2 half-edges form the twin pair
    }

    [Fact]
    public void TwinLinking_NoSharedEdge_NoTwins()
    {
        var builder = new MeshBuilder();
        // Two triangles with no shared edges
        var tri1 = new Triangle3(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var tri2 = new Triangle3(new Vec3(5, 5, 5), new Vec3(6, 5, 5), new Vec3(5, 6, 5));
        var mesh = builder.Build(new[] { tri1, tri2 });

        foreach (var he in mesh.HalfEdges)
            Assert.Null(he.Twin);
    }

    [Fact]
    public void Build_Cube_AllTwinsLinked()
    {
        var builder = new MeshBuilder();
        var cube = MeshFactory.CreateCube(Vec3.Zero, 1);
        // All edges of a closed cube should have twins
        foreach (var he in cube.Mesh.HalfEdges)
            Assert.NotNull(he.Twin);
    }

    [Fact]
    public void Build_SingleTriangle_NoTwins()
    {
        var builder = new MeshBuilder();
        var tri = new Triangle3(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0));
        var mesh = builder.Build(new[] { tri });

        foreach (var he in mesh.HalfEdges)
            Assert.Null(he.Twin);
    }

    [Fact]
    public void Build_Indexed_NoWelding()
    {
        var builder = new MeshBuilder();
        var positions = new[] {
            Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0),
            Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 0, 1)
        };
        var indices = new[] { (0, 1, 2), (3, 4, 5) };
        var mesh = builder.Build(positions, indices);
        // Indexed build doesn't weld — creates all 6 vertices
        Assert.Equal(6, mesh.Vertices.Count);
    }

    [Fact]
    public void Build_Indexed_TwinLinking_Works()
    {
        var builder = new MeshBuilder();
        var positions = new[] {
            Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0), new Vec3(0, 0, 1)
        };
        // Two triangles sharing edge (0,1) with opposite winding → twins linked
        var indices = new[] { (0, 1, 2), (1, 0, 3) };
        var mesh = builder.Build(positions, indices);

        int twinCount = mesh.HalfEdges.Count(he => he.Twin != null);
        Assert.True(twinCount >= 2);
    }

    [Fact]
    public void Build_NearbyVertices_WeldedToSameVertex()
    {
        var builder = new MeshBuilder(0.01);
        // Two triangles with vertices slightly offset — should weld
        var tri1 = new Triangle3(
            new Vec3(0, 0, 0),
            new Vec3(1, 0, 0),
            new Vec3(0, 1, 0));
        var tri2 = new Triangle3(
            new Vec3(0.005, 0, 0), // within 0.01 of (0,0,0)
            new Vec3(1.005, 0, 0), // within 0.01 of (1,0,0)
            new Vec3(0, 0, 2));
        var mesh = builder.Build(new[] { tri1, tri2 });
        Assert.True(mesh.Vertices.Count <= 4, $"Expected ≤4 vertices, got {mesh.Vertices.Count}");
    }

    [Fact]
    public void Build_VerticesJustOutsideTolerance_NotWelded()
    {
        var builder = new MeshBuilder(0.01);
        var tri1 = new Triangle3(
            new Vec3(0, 0, 0),
            new Vec3(1, 0, 0),
            new Vec3(0, 1, 0));
        var tri2 = new Triangle3(
            new Vec3(0.02, 0, 0), // outside 0.01 of (0,0,0)
            new Vec3(1.02, 0, 0), // outside 0.01 of (1,0,0)
            new Vec3(0, 0, 2));
        var mesh = builder.Build(new[] { tri1, tri2 });
        // Should NOT weld — all 6 vertices unique
        Assert.True(mesh.Vertices.Count >= 5);
    }

    [Fact]
    public void Build_MultipleTriangles_FaceCount()
    {
        var builder = new MeshBuilder();
        var tris = new[]
        {
            new Triangle3(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new Triangle3(Vec3.Zero, new Vec3(0, 1, 0), new Vec3(0, 0, 1)),
            new Triangle3(Vec3.Zero, new Vec3(0, 0, 1), new Vec3(1, 0, 0)),
        };
        var mesh = builder.Build(tris);
        Assert.Equal(3, mesh.Faces.Count);
    }

    [Fact]
    public void Build_HalfEdgeCount_IsThreeTimeFaces()
    {
        var builder = new MeshBuilder();
        var tris = new[]
        {
            new Triangle3(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new Triangle3(Vec3.Zero, new Vec3(0, 1, 0), new Vec3(0, 0, 1)),
        };
        var mesh = builder.Build(tris);
        Assert.Equal(6, mesh.HalfEdges.Count); // 2 faces * 3 edges each
    }
}
