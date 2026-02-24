using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Mesh;

/// <summary>Batch 10: MeshBuilder and MeshValidator extended tests (20 tests)</summary>
public class MeshBuilderValidatorExtTests
{
    // --- MeshBuilder ---

    [Fact]
    public void MeshBuilder_SingleTriangle()
    {
        var builder = new MeshBuilder();
        var tris = new[] { new Triangle3(Vec3.Zero, Vec3.UnitX, Vec3.UnitY) };
        var mesh = builder.Build(tris);
        Assert.Equal(1, mesh.Faces.Count);
        Assert.Equal(3, mesh.Vertices.Count);
    }

    [Fact]
    public void MeshBuilder_TwoTriangles_SharedEdge()
    {
        var builder = new MeshBuilder();
        var tris = new[]
        {
            new Triangle3(new Vec3(0,0,0), new Vec3(1,0,0), new Vec3(0,1,0)),
            new Triangle3(new Vec3(1,0,0), new Vec3(1,1,0), new Vec3(0,1,0)),
        };
        var mesh = builder.Build(tris);
        Assert.Equal(2, mesh.Faces.Count);
        Assert.Equal(4, mesh.Vertices.Count);
    }

    [Fact]
    public void MeshBuilder_VertexWelding()
    {
        var builder = new MeshBuilder(1e-6);
        var tris = new[]
        {
            new Triangle3(new Vec3(0,0,0), new Vec3(1,0,0), new Vec3(0,1,0)),
            new Triangle3(new Vec3(1.0000001,0,0), new Vec3(1,1,0), new Vec3(0.0000001,1,0)),
        };
        var mesh = builder.Build(tris);
        // With welding tolerance 1e-6, (1,0,0) and (1.0000001,0,0) should be merged
        // Similarly (0,1,0) and (0.0000001,1,0) should be merged
        Assert.Equal(4, mesh.Vertices.Count);
    }

    [Fact]
    public void MeshBuilder_NoWelding_SeperateVertices()
    {
        var builder = new MeshBuilder(0);
        var tris = new[]
        {
            new Triangle3(new Vec3(0,0,0), new Vec3(1,0,0), new Vec3(0,1,0)),
            new Triangle3(new Vec3(1,0,0), new Vec3(1,1,0), new Vec3(0,1,0)),
        };
        var mesh = builder.Build(tris);
        // Without welding, each triangle has independent vertices
        Assert.Equal(6, mesh.Vertices.Count);
    }

    [Fact]
    public void MeshBuilder_FromIndexed()
    {
        var positions = new Vec3[]
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 1, 0), new(1, 1, 0)
        };
        var indices = new (int, int, int)[] { (0, 1, 2), (1, 3, 2) };
        var builder = new MeshBuilder();
        var mesh = builder.Build(positions, indices);
        Assert.Equal(2, mesh.Faces.Count);
        Assert.Equal(4, mesh.Vertices.Count);
    }

    [Fact]
    public void MeshBuilder_LinkedTwins()
    {
        var cube = MeshFactory.CreateCube();
        Assert.True(MeshValidator.AllEdgesHaveTwins(cube.Mesh));
    }

    [Fact]
    public void MeshBuilder_CubeIsManifold()
    {
        var cube = MeshFactory.CreateCube();
        Assert.True(MeshValidator.IsEdgeManifold(cube.Mesh));
    }

    // --- MeshValidator ---

    [Fact]
    public void Validator_Cube_AllChecksPass()
    {
        var cube = MeshFactory.CreateCube();
        var result = MeshValidator.Validate(cube.Mesh);
        Assert.True(result.AllEdgesHaveTwins);
        Assert.True(result.IsEdgeManifold);
        Assert.True(result.IsConsistentlyOriented);
        Assert.True(result.HasValidFaceCycles);
        Assert.True(result.IsClosedManifold);
    }

    [Fact]
    public void Validator_Cube_EulerCharacteristic()
    {
        var cube = MeshFactory.CreateCube();
        Assert.Equal(2, MeshValidator.EulerCharacteristic(cube.Mesh));
    }

    [Fact]
    public void Validator_Cube_VertexEdgeFaceCounts()
    {
        var cube = MeshFactory.CreateCube();
        var result = MeshValidator.Validate(cube.Mesh);
        Assert.Equal(8, result.VertexCount);
        Assert.Equal(18, result.EdgeCount); // 36 half-edges / 2
        Assert.Equal(12, result.FaceCount);
    }

    [Fact]
    public void Validator_Tetrahedron_IsClosedManifold()
    {
        var tet = MeshFactory.CreateTetrahedron();
        var result = MeshValidator.Validate(tet.Mesh);
        Assert.True(result.IsClosedManifold);
        Assert.Equal(2, result.EulerCharacteristic);
    }

    [Fact]
    public void Validator_Sphere_IsClosedManifold()
    {
        var sphere = MeshFactory.CreateSphere(subdivisions: 1);
        var result = MeshValidator.Validate(sphere.Mesh);
        Assert.True(result.IsClosedManifold);
        Assert.Equal(2, result.EulerCharacteristic);
    }

    [Fact]
    public void Validator_Sphere_FaceCount()
    {
        var sphere = MeshFactory.CreateSphere(subdivisions: 0);
        Assert.Equal(20, sphere.Mesh.Faces.Count); // icosahedron
        var sphere1 = MeshFactory.CreateSphere(subdivisions: 1);
        Assert.Equal(80, sphere1.Mesh.Faces.Count); // 20*4
        var sphere2 = MeshFactory.CreateSphere(subdivisions: 2);
        Assert.Equal(320, sphere2.Mesh.Faces.Count); // 80*4
    }

    [Fact]
    public void Validator_SingleTriangle_NoTwins()
    {
        var mesh = new HalfEdgeMesh();
        var v0 = mesh.AddVertex(Vec3.Zero);
        var v1 = mesh.AddVertex(Vec3.UnitX);
        var v2 = mesh.AddVertex(Vec3.UnitY);
        mesh.AddFace(v0, v1, v2);
        Assert.False(MeshValidator.AllEdgesHaveTwins(mesh));
        Assert.False(MeshValidator.IsEdgeManifold(mesh));
    }

    [Fact]
    public void Validator_ConsistentlyOriented_Cube()
    {
        var cube = MeshFactory.CreateCube();
        Assert.True(MeshValidator.IsConsistentlyOriented(cube.Mesh));
    }

    [Fact]
    public void Validator_ValidFaceCycles_Cube()
    {
        var cube = MeshFactory.CreateCube();
        Assert.True(MeshValidator.HasValidFaceCycles(cube.Mesh));
    }

    [Fact]
    public void VolumeCalculator_UnitCube()
    {
        var cube = MeshFactory.CreateCube();
        Assert.Equal(1.0, VolumeCalculator.ComputeAbsoluteVolume(cube.Mesh), 1e-10);
    }

    [Fact]
    public void VolumeCalculator_ScaledCube()
    {
        var cube = MeshFactory.CreateCube(size: 2.0);
        Assert.Equal(8.0, VolumeCalculator.ComputeAbsoluteVolume(cube.Mesh), 1e-10);
    }

    [Fact]
    public void VolumeCalculator_OffsetCube()
    {
        var cube = MeshFactory.CreateCube(new Vec3(10, 10, 10));
        Assert.Equal(1.0, VolumeCalculator.ComputeAbsoluteVolume(cube.Mesh), 1e-10);
    }

    [Fact]
    public void VolumeCalculator_Sphere_ApproximatesFourThirdsPiR3()
    {
        var sphere = MeshFactory.CreateSphere(subdivisions: 3);
        double expected = 4.0 / 3.0 * System.Math.PI; // r=1
        double actual = VolumeCalculator.ComputeAbsoluteVolume(sphere.Mesh);
        Assert.True(System.Math.Abs(actual - expected) < 0.1,
            $"Sphere volume: expected ~{expected:F4}, got {actual:F4}");
    }
}
