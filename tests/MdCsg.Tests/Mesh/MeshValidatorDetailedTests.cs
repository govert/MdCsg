using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Mesh;

/// <summary>Phase 6: MeshValidator — detailed validation result, IsClosedManifold, boundary meshes</summary>
public class MeshValidatorDetailedTests
{
    [Fact]
    public void Cube_AllEdgesHaveTwins()
    {
        var cube = MeshFactory.CreateCube();
        Assert.True(MeshValidator.AllEdgesHaveTwins(cube.Mesh));
    }

    [Fact]
    public void Cube_IsEdgeManifold()
    {
        Assert.True(MeshValidator.IsEdgeManifold(MeshFactory.CreateCube().Mesh));
    }

    [Fact]
    public void Cube_IsConsistentlyOriented()
    {
        Assert.True(MeshValidator.IsConsistentlyOriented(MeshFactory.CreateCube().Mesh));
    }

    [Fact]
    public void Cube_HasValidFaceCycles()
    {
        Assert.True(MeshValidator.HasValidFaceCycles(MeshFactory.CreateCube().Mesh));
    }

    [Fact]
    public void Cube_EulerCharacteristic_Is2()
    {
        Assert.Equal(2, MeshValidator.EulerCharacteristic(MeshFactory.CreateCube().Mesh));
    }

    [Fact]
    public void Cube_Validate_IsClosedManifold()
    {
        var result = MeshValidator.Validate(MeshFactory.CreateCube().Mesh);
        Assert.True(result.IsClosedManifold);
        Assert.True(result.AllEdgesHaveTwins);
        Assert.True(result.IsEdgeManifold);
        Assert.True(result.IsConsistentlyOriented);
        Assert.True(result.HasValidFaceCycles);
        Assert.Equal(2, result.EulerCharacteristic);
    }

    [Fact]
    public void Sphere_IsClosedManifold()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var result = MeshValidator.Validate(sphere.Mesh);
        Assert.True(result.IsClosedManifold);
    }

    [Fact]
    public void Sphere_EulerCharacteristic_Is2()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        Assert.Equal(2, MeshValidator.EulerCharacteristic(sphere.Mesh));
    }

    [Fact]
    public void Tetrahedron_IsClosedManifold()
    {
        var tet = MeshFactory.CreateTetrahedron();
        var result = MeshValidator.Validate(tet.Mesh);
        Assert.True(result.IsClosedManifold);
    }

    [Fact]
    public void SingleTriangle_NotAllEdgesHaveTwins()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(new Vec3(1, 0, 0));
        var v2 = mesh.AddVertex(new Vec3(0, 1, 0));
        mesh.AddFace(v0, v1, v2);

        Assert.False(MeshValidator.AllEdgesHaveTwins(mesh));
    }

    [Fact]
    public void EmptyMesh_HasValidFaceCycles()
    {
        Assert.True(MeshValidator.HasValidFaceCycles(new HalfEdgeMesh()));
    }

    [Fact]
    public void EmptyMesh_EulerCharacteristic_IsZero()
    {
        Assert.Equal(0, MeshValidator.EulerCharacteristic(new HalfEdgeMesh()));
    }

    [Fact]
    public void Cube_VertexFaceEdgeCounts()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        // Cube: 8 vertices, 12 faces (triangulated), 36 half-edges
        Assert.Equal(8, mesh.Vertices.Count);
        Assert.Equal(12, mesh.Faces.Count);
        Assert.Equal(36, mesh.HalfEdges.Count);
    }

    [Fact]
    public void Sphere2_VertexCount()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        // Icosphere sub-2: 12 + (30*2 + 12*2) = 12 + 84... it's 42 + 80*2 = 162?
        // Actually: sub-0 = 12V, 20F; sub-1 = 42V, 80F; sub-2 = 162V, 320F
        Assert.Equal(162, sphere.Mesh.Vertices.Count);
        Assert.Equal(320, sphere.Mesh.Faces.Count);
    }
}
