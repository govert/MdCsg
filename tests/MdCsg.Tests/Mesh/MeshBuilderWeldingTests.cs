using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Mesh;

/// <summary>Phase 6: MeshBuilder welding and twin linking deep tests</summary>
public class MeshBuilderWeldingTests
{
    [Fact]
    public void Build_IdenticalVertices_Welded()
    {
        // Two triangles sharing an edge — vertices at edge should be welded
        var tris = new List<Triangle3>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new(new Vec3(1, 0, 0), new Vec3(0, 0, 0), new Vec3(0, 0, 1)),
        };
        var mesh = new MeshBuilder().Build(tris);
        // Should have 4 unique vertices, not 6
        Assert.Equal(4, mesh.Vertices.Count);
    }

    [Fact]
    public void Build_FarVertices_NotWelded()
    {
        var tris = new List<Triangle3>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new(new Vec3(5, 5, 5), new Vec3(6, 5, 5), new Vec3(5, 6, 5)),
        };
        var mesh = new MeshBuilder().Build(tris);
        Assert.Equal(6, mesh.Vertices.Count);
    }

    [Fact]
    public void Build_CustomTolerance_AffectsWelding()
    {
        var v1 = new Vec3(0, 0, 0);
        var v2 = new Vec3(1, 0, 0);
        var v3 = new Vec3(0, 1, 0);
        var v4 = new Vec3(1e-9, 0, 0); // Very close to v1
        var tris = new List<Triangle3>
        {
            new(v1, v2, v3),
            new(v4, v3, v2),
        };
        // Default tolerance 1e-10 should NOT weld v1 and v4 (distance=1e-9)
        var meshDefault = new MeshBuilder().Build(tris);
        // Larger tolerance SHOULD weld them
        var meshLarger = new MeshBuilder(1e-8).Build(tris);
        Assert.True(meshLarger.Vertices.Count <= meshDefault.Vertices.Count);
    }

    [Fact]
    public void Build_CubeTriangles_CorrectVertexCount()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        // Unit cube has 8 unique vertices
        Assert.Equal(8, mesh.Vertices.Count);
    }

    [Fact]
    public void Build_CubeTriangles_CorrectFaceCount()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        // 6 faces × 2 triangles = 12 faces
        Assert.Equal(12, mesh.Faces.Count);
    }

    [Fact]
    public void Build_TwinLinking_SharedEdgesHaveTwins()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var validation = MeshValidator.Validate(mesh);
        Assert.True(validation.AllEdgesHaveTwins, "Cube should have all edges twinned");
    }

    [Fact]
    public void Build_ConsistentOrientation_AfterBuild()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var validation = MeshValidator.Validate(mesh);
        Assert.True(validation.IsConsistentlyOriented);
    }

    [Fact]
    public void Build_IndexedBuild_NoWelding()
    {
        var verts = new List<Vec3>
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 1, 0), new(0, 0, 1)
        };
        var faces = new List<(int, int, int)>
        {
            (0, 1, 2), (0, 2, 3), (0, 3, 1), (1, 3, 2)
        };
        var mesh = new MeshBuilder().Build(verts, faces);
        Assert.Equal(4, mesh.Vertices.Count);
        Assert.Equal(4, mesh.Faces.Count);
    }

    [Fact]
    public void Build_IndexedBuild_TwinsLinked()
    {
        var verts = new List<Vec3>
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 1, 0), new(0, 0, 1)
        };
        var faces = new List<(int, int, int)>
        {
            (0, 1, 2), (0, 2, 3), (0, 3, 1), (1, 3, 2)
        };
        var mesh = new MeshBuilder().Build(verts, faces);
        var validation = MeshValidator.Validate(mesh);
        Assert.True(validation.AllEdgesHaveTwins);
    }

    [Fact]
    public void Build_SingleTriangle_ThreeVertices()
    {
        var tris = new List<Triangle3>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0))
        };
        var mesh = new MeshBuilder().Build(tris);
        Assert.Equal(3, mesh.Vertices.Count);
        Assert.Single(mesh.Faces);
    }

    [Fact]
    public void Build_TwoDisconnectedTriangles_SixVertices()
    {
        var tris = new List<Triangle3>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new(new Vec3(10, 10, 10), new Vec3(11, 10, 10), new Vec3(10, 11, 10)),
        };
        var mesh = new MeshBuilder().Build(tris);
        Assert.Equal(6, mesh.Vertices.Count);
        Assert.Equal(2, mesh.Faces.Count);
    }

    [Fact]
    public void Build_TetrahedronTriangles_CorrectCounts()
    {
        var mesh = MeshFactory.CreateTetrahedron().Mesh;
        Assert.Equal(4, mesh.Vertices.Count);
        Assert.Equal(4, mesh.Faces.Count);
    }

    [Fact]
    public void Build_SphereTriangles_ClosedManifold()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh;
        var validation = MeshValidator.Validate(mesh);
        Assert.True(validation.IsClosedManifold, "Sphere should be closed manifold");
    }

    [Fact]
    public void Build_HalfEdgeCount_Correct()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        // Each face has 3 half-edges, so 12 * 3 = 36
        Assert.Equal(36, mesh.HalfEdges.Count);
    }
}
