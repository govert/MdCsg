using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: CSG pipeline invariant tests — output mesh validity, face cycles, edge structure</summary>
public class CsgPipelineInvariantTests
{
    [Theory]
    [InlineData(0.3, 0.3, 0.3)]
    [InlineData(0.5, 0, 0)]
    [InlineData(0, 0.5, 0)]
    [InlineData(0, 0, 0.5)]
    public void Union_OutputHasValidFaceCycles(double dx, double dy, double dz)
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(dx, dy, dz)).Mesh);
        var result = Csg.Union(a, b);
        var validator = MeshValidator.Validate(result.Mesh);
        Assert.True(validator.HasValidFaceCycles, "Union output should have valid face cycles");
    }

    [Theory]
    [InlineData(0.3, 0.3, 0.3)]
    [InlineData(0.5, 0, 0)]
    [InlineData(0, 0.5, 0)]
    [InlineData(0, 0, 0.5)]
    public void Intersect_OutputHasValidFaceCycles(double dx, double dy, double dz)
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(dx, dy, dz)).Mesh);
        var result = Csg.Intersect(a, b);
        var validator = MeshValidator.Validate(result.Mesh);
        Assert.True(validator.HasValidFaceCycles, "Intersect output should have valid face cycles");
    }

    [Theory]
    [InlineData(0.3, 0.3, 0.3)]
    [InlineData(0.5, 0, 0)]
    [InlineData(0, 0.5, 0)]
    [InlineData(0, 0, 0.5)]
    public void Difference_OutputHasValidFaceCycles(double dx, double dy, double dz)
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(dx, dy, dz)).Mesh);
        var result = Csg.Difference(a, b);
        var validator = MeshValidator.Validate(result.Mesh);
        Assert.True(validator.HasValidFaceCycles, "Difference output should have valid face cycles");
    }

    [Fact]
    public void Union_OutputFaces_AllTriangular()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Union(a, b);
        foreach (var face in result.Mesh.Faces)
        {
            var verts = face.GetVertices();
            Assert.Equal(3, verts.Count);
        }
    }

    [Fact]
    public void Union_OutputNormals_Finite()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Union(a, b);
        foreach (var face in result.Mesh.Faces)
        {
            var n = face.Normal;
            Assert.False(double.IsNaN(n.X) || double.IsNaN(n.Y) || double.IsNaN(n.Z),
                "Face normal contains NaN");
            Assert.False(double.IsInfinity(n.X) || double.IsInfinity(n.Y) || double.IsInfinity(n.Z),
                "Face normal contains Infinity");
        }
    }

    [Fact]
    public void Union_ConsistentOrientation()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Union(a, b);
        var validation = MeshValidator.Validate(result.Mesh);
        Assert.True(validation.IsConsistentlyOriented);
    }

    [Fact]
    public void Intersect_ConsistentOrientation()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Intersect(a, b);
        var validation = MeshValidator.Validate(result.Mesh);
        Assert.True(validation.IsConsistentlyOriented);
    }

    [Fact]
    public void Difference_ConsistentOrientation()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = Csg.Difference(a, b);
        var validation = MeshValidator.Validate(result.Mesh);
        Assert.True(validation.IsConsistentlyOriented);
    }

    [Fact]
    public void SphereSphere_Union_ValidOutput()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1, 2).Mesh);
        var result = Csg.Union(a, b);
        var validation = MeshValidator.Validate(result.Mesh);
        Assert.True(validation.HasValidFaceCycles);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void CubeTetra_Difference_ValidOutput()
    {
        var a = new Solid(MeshFactory.CreateCube(Vec3.Zero, 2).Mesh);
        var b = new Solid(MeshFactory.CreateTetrahedron(new Vec3(0.5, 0.5, 0.5), 1).Mesh);
        var result = Csg.Difference(a, b);
        Assert.True(result.FaceCount > 0);
        foreach (var face in result.Mesh.Faces)
        {
            face.GetTrianglePositions(out var va, out var vb, out var vc);
            Assert.False(double.IsNaN(va.X));
            Assert.False(double.IsNaN(vb.X));
            Assert.False(double.IsNaN(vc.X));
        }
    }

    [Fact]
    public void ChainedOperations_OutputRemainValid()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0, 0)).Mesh);
        var r1 = Csg.Union(a, b);
        var solid1 = new Solid(r1.Mesh);
        var c = new Solid(MeshFactory.CreateCube(new Vec3(0, 0.5, 0)).Mesh);
        var r2 = Csg.Union(solid1, c);
        Assert.True(r2.FaceCount > 0);
        var validation = MeshValidator.Validate(r2.Mesh);
        Assert.True(validation.HasValidFaceCycles);
    }
}
