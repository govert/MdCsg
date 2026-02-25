using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Tests.Mesh;

/// <summary>Phase 6: MeshBuilder welding boundary behavior — tolerance edges, hash collisions, indexed builds</summary>
public class MeshBuilderBoundaryTests
{
    [Fact]
    public void WeldTolerance_VerticesWithinTolerance_AreMerged()
    {
        var builder = new MeshBuilder(0.01);
        var triangles = new List<Triangle3>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new(new Vec3(0.005, 0, 0), new Vec3(1.005, 0, 0), new Vec3(0.005, 1, 0)),
        };
        var mesh = builder.Build(triangles);
        // With tolerance 0.01, the second triangle's vertices should merge with the first's
        // So we should have fewer than 6 unique vertices
        Assert.True(mesh.Vertices.Count < 6, $"Expected welding, got {mesh.Vertices.Count} vertices");
    }

    [Fact]
    public void WeldTolerance_VerticesBeyondTolerance_NotMerged()
    {
        var builder = new MeshBuilder(0.001);
        var triangles = new List<Triangle3>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new(new Vec3(0.01, 0, 0), new Vec3(1.01, 0, 0), new Vec3(0.01, 1, 0)),
        };
        var mesh = builder.Build(triangles);
        // With tolerance 0.001, the offset is too large for welding
        Assert.Equal(6, mesh.Vertices.Count);
    }

    [Fact]
    public void WeldTolerance_ExactlyAtBoundary()
    {
        var builder = new MeshBuilder(0.01);
        var triangles = new List<Triangle3>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            // Second triangle shares exact vertex positions
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 0, 1)),
        };
        var mesh = builder.Build(triangles);
        // Two shared vertices should merge (0,0,0) and (1,0,0)
        Assert.Equal(4, mesh.Vertices.Count);
    }

    [Fact]
    public void TwinLinking_SharedEdge_HasTwins()
    {
        var builder = new MeshBuilder();
        // Two triangles sharing edge (0,0,0)-(1,0,0)
        var triangles = new List<Triangle3>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new(new Vec3(1, 0, 0), new Vec3(0, 0, 0), new Vec3(0, 0, 1)),
        };
        var mesh = builder.Build(triangles);
        // The shared edge should have twin links
        int twinCount = 0;
        foreach (var he in mesh.HalfEdges)
            if (he.Twin != null) twinCount++;
        Assert.True(twinCount >= 2, $"Expected at least 2 twin-linked edges, got {twinCount}");
    }

    [Fact]
    public void TwinLinking_NoSharedEdge_NoTwins()
    {
        var builder = new MeshBuilder();
        // Two disjoint triangles
        var triangles = new List<Triangle3>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new(new Vec3(10, 10, 10), new Vec3(11, 10, 10), new Vec3(10, 11, 10)),
        };
        var mesh = builder.Build(triangles);
        int twinCount = 0;
        foreach (var he in mesh.HalfEdges)
            if (he.Twin != null) twinCount++;
        Assert.Equal(0, twinCount);
    }

    [Fact]
    public void Build_SingleTriangle_3Vertices3HalfEdges1Face()
    {
        var builder = new MeshBuilder();
        var mesh = builder.Build(new List<Triangle3>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0))
        });
        Assert.Equal(3, mesh.Vertices.Count);
        Assert.Equal(3, mesh.HalfEdges.Count);
        Assert.Equal(1, mesh.Faces.Count);
    }

    [Fact]
    public void Build_EmptyList_EmptyMesh()
    {
        var builder = new MeshBuilder();
        var mesh = builder.Build(new List<Triangle3>());
        Assert.Equal(0, mesh.Vertices.Count);
        Assert.Equal(0, mesh.HalfEdges.Count);
        Assert.Equal(0, mesh.Faces.Count);
    }

    [Fact]
    public void Build_Indexed_CorrectVertexCount()
    {
        var builder = new MeshBuilder();
        var positions = new List<Vec3>
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 1, 0), new(1, 1, 0)
        };
        var indices = new List<(int, int, int)>
        {
            (0, 1, 2),
            (1, 3, 2)
        };
        var mesh = builder.Build(positions, indices);
        Assert.Equal(4, mesh.Vertices.Count);
        Assert.Equal(2, mesh.Faces.Count);
    }

    [Fact]
    public void Build_Indexed_TwinLinked()
    {
        var builder = new MeshBuilder();
        var positions = new List<Vec3>
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 1, 0), new(1, 1, 0)
        };
        var indices = new List<(int, int, int)>
        {
            (0, 1, 2),
            (1, 3, 2)
        };
        var mesh = builder.Build(positions, indices);
        // Shared edge (1→2 and 2→1) should have twins
        int twinCount = 0;
        foreach (var he in mesh.HalfEdges)
            if (he.Twin != null) twinCount++;
        Assert.True(twinCount >= 2);
    }

    [Fact]
    public void CubeTriangles_12Faces()
    {
        var builder = new MeshBuilder();
        var cubeTriangles = GetCubeTriangles();
        var mesh = builder.Build(cubeTriangles);
        Assert.Equal(12, mesh.Faces.Count);
    }

    [Fact]
    public void CubeTriangles_8Vertices()
    {
        var builder = new MeshBuilder();
        var cubeTriangles = GetCubeTriangles();
        var mesh = builder.Build(cubeTriangles);
        Assert.Equal(8, mesh.Vertices.Count);
    }

    [Fact]
    public void CubeTriangles_ValidFaceCycles()
    {
        var builder = new MeshBuilder();
        var cubeTriangles = GetCubeTriangles();
        var mesh = builder.Build(cubeTriangles);
        Assert.True(MeshValidator.HasValidFaceCycles(mesh));
    }

    [Fact]
    public void CubeTriangles_ConsistentOrientation()
    {
        var builder = new MeshBuilder();
        var cubeTriangles = GetCubeTriangles();
        var mesh = builder.Build(cubeTriangles);
        Assert.True(MeshValidator.IsConsistentlyOriented(mesh));
    }

    [Fact]
    public void DefaultTolerance_1e10()
    {
        var builder = new MeshBuilder();
        // Build with two triangles sharing a vertex at exact same position
        var triangles = new List<Triangle3>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new(new Vec3(0, 0, 0), new Vec3(0, 0, 1), new Vec3(1, 0, 0)),
        };
        var mesh = builder.Build(triangles);
        // Default tolerance should weld the shared vertices
        Assert.True(mesh.Vertices.Count <= 4);
    }

    [Fact]
    public void NegativeCoordinates_StillWeld()
    {
        var builder = new MeshBuilder();
        var triangles = new List<Triangle3>
        {
            new(new Vec3(-1, -1, -1), new Vec3(0, -1, -1), new Vec3(-1, 0, -1)),
            new(new Vec3(-1, -1, -1), new Vec3(-1, 0, -1), new Vec3(-1, -1, 0)),
        };
        var mesh = builder.Build(triangles);
        // Shared edge should be welded
        Assert.True(mesh.Vertices.Count <= 4);
    }

    private static List<Triangle3> GetCubeTriangles()
    {
        var v = new Vec3[]
        {
            new(0,0,0), new(1,0,0), new(1,1,0), new(0,1,0),
            new(0,0,1), new(1,0,1), new(1,1,1), new(0,1,1),
        };
        return new List<Triangle3>
        {
            // Front (Z=1)
            new(v[4], v[5], v[6]), new(v[4], v[6], v[7]),
            // Back (Z=0)
            new(v[1], v[0], v[3]), new(v[1], v[3], v[2]),
            // Right (X=1)
            new(v[5], v[1], v[2]), new(v[5], v[2], v[6]),
            // Left (X=0)
            new(v[0], v[4], v[7]), new(v[0], v[7], v[3]),
            // Top (Y=1)
            new(v[3], v[7], v[6]), new(v[3], v[6], v[2]),
            // Bottom (Y=0)
            new(v[0], v[1], v[5]), new(v[0], v[5], v[4]),
        };
    }
}
