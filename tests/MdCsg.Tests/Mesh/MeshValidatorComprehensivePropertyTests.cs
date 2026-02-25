using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Mesh;

/// <summary>Phase 6: MeshValidator — comprehensive validation of cube, sphere, tetrahedron topological properties</summary>
public class MeshValidatorComprehensivePropertyTests
{
    [Fact]
    public void Cube_IsClosedManifold()
    {
        var cube = MeshFactory.CreateCube();
        var result = MeshValidator.Validate(cube.Mesh);
        Assert.True(result.IsClosedManifold);
    }

    [Fact]
    public void Cube_AllEdgesHaveTwins()
    {
        var cube = MeshFactory.CreateCube();
        Assert.True(MeshValidator.AllEdgesHaveTwins(cube.Mesh));
    }

    [Fact]
    public void Cube_IsEdgeManifold()
    {
        var cube = MeshFactory.CreateCube();
        Assert.True(MeshValidator.IsEdgeManifold(cube.Mesh));
    }

    [Fact]
    public void Cube_IsConsistentlyOriented()
    {
        var cube = MeshFactory.CreateCube();
        Assert.True(MeshValidator.IsConsistentlyOriented(cube.Mesh));
    }

    [Fact]
    public void Cube_HasValidFaceCycles()
    {
        var cube = MeshFactory.CreateCube();
        Assert.True(MeshValidator.HasValidFaceCycles(cube.Mesh));
    }

    [Fact]
    public void Cube_EulerCharacteristic_Is2()
    {
        var cube = MeshFactory.CreateCube();
        Assert.Equal(2, MeshValidator.EulerCharacteristic(cube.Mesh));
    }

    [Fact]
    public void Sphere_Subdiv2_IsClosedManifold()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var result = MeshValidator.Validate(sphere.Mesh);
        Assert.True(result.IsClosedManifold);
    }

    [Fact]
    public void Sphere_Subdiv2_EulerCharacteristic_Is2()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        Assert.Equal(2, MeshValidator.EulerCharacteristic(sphere.Mesh));
    }

    [Fact]
    public void Sphere_Subdiv3_IsClosedManifold()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 3);
        var result = MeshValidator.Validate(sphere.Mesh);
        Assert.True(result.IsClosedManifold);
    }

    [Fact]
    public void Sphere_Subdiv3_EulerCharacteristic_Is2()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 3);
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
    public void Tetrahedron_EulerCharacteristic_Is2()
    {
        var tet = MeshFactory.CreateTetrahedron();
        Assert.Equal(2, MeshValidator.EulerCharacteristic(tet.Mesh));
    }

    [Fact]
    public void Tetrahedron_Counts()
    {
        var tet = MeshFactory.CreateTetrahedron();
        var result = MeshValidator.Validate(tet.Mesh);
        Assert.Equal(4, result.VertexCount);
        Assert.Equal(4, result.FaceCount);
        Assert.Equal(6, result.EdgeCount); // 4 faces * 3 half-edges / 2
    }

    [Fact]
    public void Cube_Counts()
    {
        var cube = MeshFactory.CreateCube();
        var result = MeshValidator.Validate(cube.Mesh);
        Assert.Equal(8, result.VertexCount);
        Assert.Equal(12, result.FaceCount); // 6 faces * 2 triangles
        Assert.Equal(18, result.EdgeCount); // 12*3/2
    }

    [Fact]
    public void Validate_ResultFields_AllPopulated()
    {
        var cube = MeshFactory.CreateCube();
        var result = MeshValidator.Validate(cube.Mesh);
        Assert.True(result.AllEdgesHaveTwins);
        Assert.True(result.IsEdgeManifold);
        Assert.True(result.IsConsistentlyOriented);
        Assert.True(result.HasValidFaceCycles);
        Assert.True(result.VertexCount > 0);
        Assert.True(result.EdgeCount > 0);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void EmptyMesh_HasValidFaceCycles()
    {
        var mesh = new HalfEdgeMesh();
        Assert.True(MeshValidator.HasValidFaceCycles(mesh));
    }

    [Fact]
    public void EmptyMesh_AllEdgesHaveTwins()
    {
        var mesh = new HalfEdgeMesh();
        Assert.True(MeshValidator.AllEdgesHaveTwins(mesh));
    }

    [Fact]
    public void EmptyMesh_EulerCharacteristic_IsZero()
    {
        var mesh = new HalfEdgeMesh();
        Assert.Equal(0, MeshValidator.EulerCharacteristic(mesh));
    }

    [Fact]
    public void OffsetCube_IsClosedManifold()
    {
        var cube = MeshFactory.CreateCube(new Vec3(100, 200, 300));
        var result = MeshValidator.Validate(cube.Mesh);
        Assert.True(result.IsClosedManifold);
    }

    [Fact]
    public void LargeSphere_TwinPairs_Consistent()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 5.0, 3);
        foreach (var he in sphere.Mesh.HalfEdges)
        {
            Assert.NotNull(he.Twin);
            Assert.Same(he, he.Twin.Twin);
        }
    }

    [Fact]
    public void Sphere_Subdiv2_Counts()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        var result = MeshValidator.Validate(sphere.Mesh);
        Assert.Equal(320, result.FaceCount); // 20 * 4^2
        Assert.Equal(480, result.EdgeCount); // F*3/2 for closed manifold
        Assert.Equal(162, result.VertexCount); // V = 2 - Euler + E - F + 2 = 2 + 480 - 320 = 162
    }

    [Fact]
    public void Cube_TwinOriginTarget_Consistency()
    {
        var cube = MeshFactory.CreateCube();
        foreach (var he in cube.Mesh.HalfEdges)
        {
            // he goes from Origin to Target
            // Twin should go from Target to Origin
            Assert.Equal(he.Origin.Id, he.Twin!.Target.Id);
            Assert.Equal(he.Target.Id, he.Twin.Origin.Id);
        }
    }

    [Fact]
    public void Sphere_AllFaces_HaveEdge()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        foreach (var face in sphere.Mesh.Faces)
        {
            Assert.NotNull(face.Edge);
            Assert.Equal(face, face.Edge.Face);
        }
    }

    [Fact]
    public void Cube_NextPrev_Consistent()
    {
        var cube = MeshFactory.CreateCube();
        foreach (var he in cube.Mesh.HalfEdges)
        {
            Assert.Same(he, he.Next.Prev);
            Assert.Same(he, he.Prev.Next);
        }
    }
}
