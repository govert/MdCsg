using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Mesh;

/// <summary>Phase 6: MeshValidator — Euler characteristic, manifold checks, validation summary</summary>
public class MeshValidatorEulerPropertyTests
{
    [Fact]
    public void Cube_EulerCharacteristic_Is2()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        Assert.Equal(2, MeshValidator.EulerCharacteristic(mesh));
    }

    [Fact]
    public void Tetrahedron_EulerCharacteristic_Is2()
    {
        var mesh = MeshFactory.CreateTetrahedron().Mesh;
        Assert.Equal(2, MeshValidator.EulerCharacteristic(mesh));
    }

    [Fact]
    public void Sphere_EulerCharacteristic_Is2()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh;
        Assert.Equal(2, MeshValidator.EulerCharacteristic(mesh));
    }

    [Fact]
    public void Cube_IsClosedManifold()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var result = MeshValidator.Validate(mesh);
        Assert.True(result.IsClosedManifold,
            $"Cube should be closed manifold. Twins={result.AllEdgesHaveTwins}, Manifold={result.IsEdgeManifold}, Oriented={result.IsConsistentlyOriented}, Cycles={result.HasValidFaceCycles}");
    }

    [Fact]
    public void Tetrahedron_IsClosedManifold()
    {
        var mesh = MeshFactory.CreateTetrahedron().Mesh;
        var result = MeshValidator.Validate(mesh);
        Assert.True(result.IsClosedManifold);
    }

    [Fact]
    public void Sphere_IsClosedManifold()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh;
        var result = MeshValidator.Validate(mesh);
        Assert.True(result.IsClosedManifold);
    }

    [Fact]
    public void SingleFace_NotClosedManifold()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 0));
        mesh.AddFace(v0, v1, v2);

        Assert.False(MeshValidator.AllEdgesHaveTwins(mesh));
    }

    [Fact]
    public void SingleFace_HasValidFaceCycles()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 0));
        mesh.AddFace(v0, v1, v2);

        Assert.True(MeshValidator.HasValidFaceCycles(mesh));
    }

    [Fact]
    public void Cube_AllEdgesHaveTwins()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        Assert.True(MeshValidator.AllEdgesHaveTwins(mesh));
    }

    [Fact]
    public void Cube_IsConsistentlyOriented()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        Assert.True(MeshValidator.IsConsistentlyOriented(mesh));
    }

    [Fact]
    public void Cube_IsEdgeManifold()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        Assert.True(MeshValidator.IsEdgeManifold(mesh));
    }

    [Fact]
    public void Validate_ReportsCorrectCounts_Cube()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var result = MeshValidator.Validate(mesh);
        Assert.Equal(mesh.Vertices.Count, result.VertexCount);
        Assert.Equal(mesh.HalfEdges.Count / 2, result.EdgeCount);
        Assert.Equal(mesh.Faces.Count, result.FaceCount);
    }

    [Fact]
    public void Validate_ReportsCorrectCounts_Tetrahedron()
    {
        var mesh = MeshFactory.CreateTetrahedron().Mesh;
        var result = MeshValidator.Validate(mesh);
        // Tetrahedron: V=4, E=6, F=4 → χ = 4-6+4 = 2
        Assert.Equal(4, result.VertexCount);
        Assert.Equal(6, result.EdgeCount);
        Assert.Equal(4, result.FaceCount);
        Assert.Equal(2, result.EulerCharacteristic);
    }

    [Fact]
    public void EulerFormula_VMinusEPlusF()
    {
        // Verify V - E + F = Euler for several shapes
        var shapes = new[]
        {
            MeshFactory.CreateCube().Mesh,
            MeshFactory.CreateTetrahedron().Mesh,
            MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1).Mesh,
            MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh,
        };

        foreach (var mesh in shapes)
        {
            int v = mesh.Vertices.Count;
            int e = mesh.HalfEdges.Count / 2;
            int f = mesh.Faces.Count;
            int euler = MeshValidator.EulerCharacteristic(mesh);
            Assert.Equal(v - e + f, euler);
        }
    }
}
