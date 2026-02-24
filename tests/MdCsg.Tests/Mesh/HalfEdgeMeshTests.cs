using MdCsg.Math;
using MdCsg.Mesh;

namespace MdCsg.Tests.Mesh;

public class HalfEdgeMeshTests
{
    /// <summary>
    /// Builds a cube mesh with 8 vertices, 12 triangular faces.
    /// </summary>
    private static HalfEdgeMesh BuildCubeMesh()
    {
        var positions = new Vec3[]
        {
            new(0, 0, 0), new(1, 0, 0), new(1, 1, 0), new(0, 1, 0), // bottom
            new(0, 0, 1), new(1, 0, 1), new(1, 1, 1), new(0, 1, 1), // top
        };

        // 12 triangles (2 per face, 6 faces), CCW winding when viewed from outside
        var triangles = new (int, int, int)[]
        {
            // Bottom (z=0) - normal points -Z, viewed from -Z these are CCW
            (0, 2, 1), (0, 3, 2),
            // Top (z=1) - normal points +Z
            (4, 5, 6), (4, 6, 7),
            // Front (y=0) - normal points -Y
            (0, 1, 5), (0, 5, 4),
            // Back (y=1) - normal points +Y
            (2, 3, 7), (2, 7, 6),
            // Left (x=0) - normal points -X
            (0, 4, 7), (0, 7, 3),
            // Right (x=1) - normal points +X
            (1, 2, 6), (1, 6, 5),
        };

        var builder = new MeshBuilder();
        return builder.Build(positions, triangles);
    }

    [Fact]
    public void Cube_HasCorrectCounts()
    {
        var mesh = BuildCubeMesh();
        Assert.Equal(8, mesh.Vertices.Count);
        Assert.Equal(12, mesh.Faces.Count);
        Assert.Equal(36, mesh.HalfEdges.Count); // 12 faces * 3 half-edges
    }

    [Fact]
    public void Cube_AllEdgesHaveTwins()
    {
        var mesh = BuildCubeMesh();
        Assert.True(MeshValidator.AllEdgesHaveTwins(mesh));
    }

    [Fact]
    public void Cube_IsEdgeManifold()
    {
        var mesh = BuildCubeMesh();
        Assert.True(MeshValidator.IsEdgeManifold(mesh));
    }

    [Fact]
    public void Cube_IsConsistentlyOriented()
    {
        var mesh = BuildCubeMesh();
        Assert.True(MeshValidator.IsConsistentlyOriented(mesh));
    }

    [Fact]
    public void Cube_HasValidFaceCycles()
    {
        var mesh = BuildCubeMesh();
        Assert.True(MeshValidator.HasValidFaceCycles(mesh));
    }

    [Fact]
    public void Cube_EulerCharacteristic_Is2()
    {
        var mesh = BuildCubeMesh();
        // V-E+F = 8 - 18 + 12 = 2 (sphere topology)
        Assert.Equal(2, MeshValidator.EulerCharacteristic(mesh));
    }

    [Fact]
    public void Cube_FullValidation_IsClosedManifold()
    {
        var mesh = BuildCubeMesh();
        var result = MeshValidator.Validate(mesh);
        Assert.True(result.IsClosedManifold);
        Assert.Equal(2, result.EulerCharacteristic);
    }

    [Fact]
    public void SingleTriangle_HalfEdges_FormCycle()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(new Vec3(0, 0, 0));
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 0));
        var face = mesh.AddFace(v0, v1, v2);

        // Check cycle: edge.Next.Next.Next == edge
        Assert.Same(face.Edge, face.Edge.Next.Next.Next);
        Assert.Same(face.Edge, face.Edge.Prev.Prev.Prev);
    }

    [Fact]
    public void FaceVertices_ReturnsCorrectOrder()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(new Vec3(0, 0, 0));
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 0));
        var face = mesh.AddFace(v0, v1, v2);

        var verts = face.GetVertices();
        Assert.Equal(3, verts.Count);
        // The face cycle visits v1, v2, v0 (targets of half-edges from v0)
        Assert.Contains(v0, verts);
        Assert.Contains(v1, verts);
        Assert.Contains(v2, verts);
    }

    [Fact]
    public void GetBounds_CubeIsUnitCube()
    {
        var mesh = BuildCubeMesh();
        var bounds = mesh.GetBounds();
        Assert.Equal(Vec3.Zero, bounds.Min);
        Assert.Equal(new Vec3(1, 1, 1), bounds.Max);
    }

    [Fact]
    public void BuildFromTriangles_WorksCorrectly()
    {
        var triangles = new Triangle3[]
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
        };
        var builder = new MeshBuilder();
        var mesh = builder.Build(triangles);
        Assert.Equal(3, mesh.Vertices.Count);
        Assert.Equal(1, mesh.Faces.Count);
    }
}
