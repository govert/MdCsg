using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Mesh;

/// <summary>Phase 6: MeshValidator topology — Euler characteristic, face cycles, validation result properties</summary>
public class MeshValidatorTopologyTests
{
    [Fact]
    public void Cube_EulerCharacteristic_Is2()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
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
    public void Cube_AllEdgesHaveTwins()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        Assert.True(MeshValidator.AllEdgesHaveTwins(mesh));
    }

    [Fact]
    public void Cube_IsEdgeManifold()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        Assert.True(MeshValidator.IsEdgeManifold(mesh));
    }

    [Fact]
    public void Cube_IsConsistentlyOriented()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        Assert.True(MeshValidator.IsConsistentlyOriented(mesh));
    }

    [Fact]
    public void Cube_HasValidFaceCycles()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        Assert.True(MeshValidator.HasValidFaceCycles(mesh));
    }

    [Fact]
    public void Validate_Cube_CorrectCounts()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var result = MeshValidator.Validate(mesh);
        Assert.Equal(12, result.FaceCount); // 6 faces * 2 triangles
        Assert.Equal(8, result.VertexCount);
        // Cube: 12 faces, 8 vertices, E = V - χ + F = 8 - 2 + 12 = 18
        Assert.Equal(18, result.EdgeCount);
    }

    [Fact]
    public void Validate_Sphere_CorrectCounts()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1).Mesh;
        var result = MeshValidator.Validate(mesh);
        Assert.True(result.FaceCount > 0);
        Assert.True(result.VertexCount > 0);
        Assert.True(result.EdgeCount > 0);
        // Euler: V - E + F = 2
        Assert.Equal(2, result.VertexCount - result.EdgeCount + result.FaceCount);
    }

    [Fact]
    public void CsgResult_HasValidFaceCycles()
    {
        var a = new MdCsg.Api.Solid(MeshFactory.CreateCube().Mesh);
        var b = new MdCsg.Api.Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = MdCsg.Api.Csg.Union(a, b);
        var valid = MeshValidator.Validate(result.Mesh);
        Assert.True(valid.HasValidFaceCycles);
        Assert.True(valid.FaceCount > 0);
    }

    [Fact]
    public void CsgIntersection_HasValidFaceCycles()
    {
        var a = new MdCsg.Api.Solid(MeshFactory.CreateCube().Mesh);
        var b = new MdCsg.Api.Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = MdCsg.Api.Csg.Intersect(a, b);
        var valid = MeshValidator.Validate(result.Mesh);
        Assert.True(valid.HasValidFaceCycles);
    }

    [Fact]
    public void CsgDifference_HasValidFaceCycles()
    {
        var a = new MdCsg.Api.Solid(MeshFactory.CreateCube().Mesh);
        var b = new MdCsg.Api.Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = MdCsg.Api.Csg.Difference(a, b);
        var valid = MeshValidator.Validate(result.Mesh);
        Assert.True(valid.HasValidFaceCycles);
    }

    [Fact]
    public void TwinSymmetry_AllHalfEdges()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var he in mesh.HalfEdges)
        {
            Assert.NotNull(he.Twin);
            Assert.Equal(he, he.Twin.Twin);
        }
    }

    [Fact]
    public void NextPrev_Cycle_AllFaces()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        foreach (var face in mesh.Faces)
        {
            var start = face.Edge;
            var current = start;
            int count = 0;
            do
            {
                Assert.Equal(current, current.Next.Prev);
                Assert.Equal(current, current.Prev.Next);
                current = current.Next;
                count++;
            } while (current != start);
            Assert.Equal(3, count); // triangular faces
        }
    }

    [Fact]
    public void Sphere_TwinConsistency()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2).Mesh;
        foreach (var he in mesh.HalfEdges)
        {
            Assert.NotNull(he.Twin);
            // Twin vertices opposite
            Assert.Equal(he.Origin.Id, he.Twin.Target.Id);
            Assert.Equal(he.Target.Id, he.Twin.Origin.Id);
        }
    }

    [Fact]
    public void HighSubdivisionSphere_IsClosedManifold()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 3).Mesh;
        var result = MeshValidator.Validate(mesh);
        Assert.True(result.IsClosedManifold);
        Assert.Equal(2, result.EulerCharacteristic);
    }

    [Fact]
    public void DisjointUnion_HasValidFaceCycles()
    {
        var a = new MdCsg.Api.Solid(MeshFactory.CreateCube().Mesh);
        var b = new MdCsg.Api.Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);
        var result = MdCsg.Api.Csg.Union(a, b);
        Assert.True(MeshValidator.HasValidFaceCycles(result.Mesh));
    }

    [Fact]
    public void Validate_ResultRecord_HasAllProperties()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var result = MeshValidator.Validate(mesh);
        // All properties should be accessible
        _ = result.AllEdgesHaveTwins;
        _ = result.IsEdgeManifold;
        _ = result.IsConsistentlyOriented;
        _ = result.HasValidFaceCycles;
        _ = result.EulerCharacteristic;
        _ = result.VertexCount;
        _ = result.EdgeCount;
        _ = result.FaceCount;
        _ = result.IsClosedManifold;
        Assert.NotNull(result.ToString());
    }
}
