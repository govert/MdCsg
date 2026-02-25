using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG round-trip tests — verify output meshes are valid closed manifolds</summary>
public class CsgRoundTripTests
{
    [Fact]
    public void Union_Output_HasValidFaceCycles()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Union(a, b);
        var validation = MeshValidator.Validate(result.Mesh);
        Assert.True(validation.HasValidFaceCycles);
        Assert.True(validation.FaceCount > 0);
    }

    [Fact]
    public void Intersect_Output_HasValidFaceCycles()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Intersect(a, b);
        var validation = MeshValidator.Validate(result.Mesh);
        Assert.True(validation.HasValidFaceCycles);
        Assert.True(validation.FaceCount > 0);
    }

    [Fact]
    public void Difference_Output_HasValidFaceCycles()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Difference(a, b);
        var validation = MeshValidator.Validate(result.Mesh);
        Assert.True(validation.HasValidFaceCycles);
        Assert.True(validation.FaceCount > 0);
    }

    [Fact]
    public void Union_Output_ConsistentlyOriented()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(MeshValidator.IsConsistentlyOriented(result.Mesh));
    }

    [Fact]
    public void Intersect_Output_Counts_Valid()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Intersect(a, b);
        var validation = MeshValidator.Validate(result.Mesh);
        Assert.True(validation.VertexCount > 0);
        Assert.True(validation.FaceCount > 0);
        Assert.True(validation.EdgeCount > 0);
    }

    [Fact]
    public void Difference_Output_ConsistentlyOriented()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Difference(a, b);
        Assert.True(MeshValidator.IsConsistentlyOriented(result.Mesh));
    }

    [Fact]
    public void Union_SphereCube_ValidMesh()
    {
        var s = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.6, 2).Mesh);
        var c = new Solid(MeshFactory.CreateCube().Mesh);
        var result = Csg.Union(s, c);
        Assert.True(result.FaceCount > 0);
        Assert.True(result.VertexCount > 0);
    }

    [Fact]
    public void Intersect_SphereCube_ValidMesh()
    {
        var s = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.6, 2).Mesh);
        var c = new Solid(MeshFactory.CreateCube().Mesh);
        var result = Csg.Intersect(s, c);
        Assert.True(result.FaceCount > 0);
        Assert.True(result.VertexCount > 0);
    }

    [Fact]
    public void Union_Output_NoNaN()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Union(a, b);
        foreach (var v in result.Mesh.Vertices)
        {
            Assert.False(double.IsNaN(v.Position.X));
            Assert.False(double.IsNaN(v.Position.Y));
            Assert.False(double.IsNaN(v.Position.Z));
        }
    }

    [Fact]
    public void CsgResult_CanBeUsedAsInput()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var union = Csg.Union(a, b);
        // Use the output as input for another CSG operation
        var c = new Solid(MeshFactory.CreateCube(new Vec3(-0.5, -0.5, -0.5)).Mesh);
        var result = Csg.Union(new Solid(union.Mesh), c);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void CsgResult_Metadata_Populated()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.PatchCountA > 0);
        Assert.True(result.PatchCountB > 0);
        Assert.True(result.IntersectionSegmentCount > 0);
    }

    [Fact]
    public void ThreeOperations_ChainedResult_Valid()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var c = new Solid(MeshFactory.CreateCube(new Vec3(-0.5, -0.5, -0.5)).Mesh);

        var ab = Csg.Union(a, b);
        var abMinusC = Csg.Difference(new Solid(ab.Mesh), c);
        Assert.True(abMinusC.FaceCount > 0);
    }
}
