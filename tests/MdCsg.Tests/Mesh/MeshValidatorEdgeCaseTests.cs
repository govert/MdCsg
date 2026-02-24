using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Mesh;

/// <summary>Phase 6: MeshValidator edge cases and properties</summary>
public class MeshValidatorEdgeCaseTests
{
    [Fact]
    public void Cube_AllEdgesHaveTwins_Static()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        Assert.True(MeshValidator.AllEdgesHaveTwins(mesh));
    }

    [Fact]
    public void Cube_IsEdgeManifold_Static()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        Assert.True(MeshValidator.IsEdgeManifold(mesh));
    }

    [Fact]
    public void Cube_IsConsistentlyOriented_Static()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        Assert.True(MeshValidator.IsConsistentlyOriented(mesh));
    }

    [Fact]
    public void Cube_HasValidFaceCycles_Static()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        Assert.True(MeshValidator.HasValidFaceCycles(mesh));
    }

    [Fact]
    public void Cube_EulerCharacteristic_Static()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        Assert.Equal(2, MeshValidator.EulerCharacteristic(mesh));
    }

    [Fact]
    public void Sphere_AllValidationsPassing()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh;
        var result = MeshValidator.Validate(mesh);
        Assert.True(result.AllEdgesHaveTwins);
        Assert.True(result.IsEdgeManifold);
        Assert.True(result.IsConsistentlyOriented);
        Assert.True(result.HasValidFaceCycles);
        Assert.Equal(2, result.EulerCharacteristic);
        Assert.True(result.IsClosedManifold);
    }

    [Fact]
    public void Tetrahedron_EulerCharacteristic2()
    {
        var mesh = MeshFactory.CreateTetrahedron().Mesh;
        Assert.Equal(2, MeshValidator.EulerCharacteristic(mesh));
    }

    [Fact]
    public void Tetrahedron_IsClosedManifold()
    {
        var mesh = MeshFactory.CreateTetrahedron().Mesh;
        var result = MeshValidator.Validate(mesh);
        Assert.True(result.IsClosedManifold);
    }

    [Fact]
    public void Cube_VertexEdgeFaceCount()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var result = MeshValidator.Validate(mesh);
        Assert.Equal(8, result.VertexCount);
        Assert.Equal(18, result.EdgeCount); // 12 tri faces * 3 edges / 2
        Assert.Equal(12, result.FaceCount);
    }

    [Fact]
    public void Tetrahedron_VertexEdgeFaceCount()
    {
        var mesh = MeshFactory.CreateTetrahedron().Mesh;
        var result = MeshValidator.Validate(mesh);
        Assert.Equal(4, result.VertexCount);
        Assert.Equal(6, result.EdgeCount); // 4 tri faces * 3 edges / 2
        Assert.Equal(4, result.FaceCount);
    }

    [Fact]
    public void Sphere_Subdivide2_EulerCharacteristic2()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh;
        Assert.Equal(2, MeshValidator.EulerCharacteristic(mesh));
    }

    [Fact]
    public void Sphere_Subdivide3_EulerCharacteristic2()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 3).Mesh;
        Assert.Equal(2, MeshValidator.EulerCharacteristic(mesh));
    }

    [Fact]
    public void CsgUnion_Result_ValidFaceCycles()
    {
        var a = new MdCsg.Api.Solid(MeshFactory.CreateCube().Mesh);
        var b = new MdCsg.Api.Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var r = MdCsg.Api.Csg.Union(a, b);
        Assert.True(MeshValidator.HasValidFaceCycles(r.Mesh));
    }

    [Fact]
    public void CsgIntersection_Result_ValidFaceCycles()
    {
        var a = new MdCsg.Api.Solid(MeshFactory.CreateCube().Mesh);
        var b = new MdCsg.Api.Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var r = MdCsg.Api.Csg.Intersect(a, b);
        Assert.True(MeshValidator.HasValidFaceCycles(r.Mesh));
    }

    [Fact]
    public void CsgDifference_Result_ValidFaceCycles()
    {
        var a = new MdCsg.Api.Solid(MeshFactory.CreateCube().Mesh);
        var b = new MdCsg.Api.Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var r = MdCsg.Api.Csg.Difference(a, b);
        Assert.True(MeshValidator.HasValidFaceCycles(r.Mesh));
    }

    [Fact]
    public void MeshValidationResult_IsClosedManifold_AllRequired()
    {
        // Manually construct result to verify IsClosedManifold logic
        var result = new MeshValidationResult
        {
            AllEdgesHaveTwins = true,
            IsEdgeManifold = true,
            IsConsistentlyOriented = true,
            HasValidFaceCycles = true,
            EulerCharacteristic = 2,
            VertexCount = 8,
            EdgeCount = 18,
            FaceCount = 12
        };
        Assert.True(result.IsClosedManifold);
    }

    [Fact]
    public void MeshValidationResult_NotClosedManifold_WhenNoTwins()
    {
        var result = new MeshValidationResult
        {
            AllEdgesHaveTwins = false,
            IsEdgeManifold = true,
            IsConsistentlyOriented = true,
            HasValidFaceCycles = true,
            EulerCharacteristic = 2,
            VertexCount = 8,
            EdgeCount = 18,
            FaceCount = 12
        };
        Assert.False(result.IsClosedManifold);
    }

    [Fact]
    public void MeshValidationResult_NotClosedManifold_WhenNotManifold()
    {
        var result = new MeshValidationResult
        {
            AllEdgesHaveTwins = true,
            IsEdgeManifold = false,
            IsConsistentlyOriented = true,
            HasValidFaceCycles = true,
            EulerCharacteristic = 2,
            VertexCount = 8,
            EdgeCount = 18,
            FaceCount = 12
        };
        Assert.False(result.IsClosedManifold);
    }

    [Fact]
    public void TwoWeldedTriangles_OpenMesh_EulerCharacteristic()
    {
        // Open mesh: 2 triangles sharing an edge
        // In half-edge representation: each triangle has 3 half-edges (6 total)
        // Only the shared edge has twin linkage, boundary edges don't
        // EulerCharacteristic = V - HalfEdges/2 + F
        var tris = new Triangle3[]
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new(new Vec3(1, 0, 0), new Vec3(1, 1, 0), new Vec3(0, 1, 0))
        };
        var builder = new MeshBuilder(1e-8);
        var mesh = builder.Build(tris);
        int euler = MeshValidator.EulerCharacteristic(mesh);
        // V=4, HalfEdges=6, E=HalfEdges/2=3, F=2 → euler = 4-3+2 = 3
        Assert.Equal(3, euler);
    }
}
