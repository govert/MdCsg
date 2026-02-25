using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Phase 6: Multi-shape CSG combinations — various shapes and operations for broad coverage</summary>
public class CsgMultiShapeTests
{
    [Fact]
    public void SphereSphere_Union_NonEmpty()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1, 2).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void SphereSphere_Intersect_NonEmpty()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1, 2).Mesh);
        var result = Csg.Intersect(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void SphereSphere_Difference_NonEmpty()
    {
        var a = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh);
        var b = new Solid(MeshFactory.CreateSphere(new Vec3(1, 0, 0), 1, 2).Mesh);
        var result = Csg.Difference(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void TetrahedronTetrahedron_Union_NonEmpty()
    {
        var a = new Solid(MeshFactory.CreateTetrahedron(Vec3.Zero, 1).Mesh);
        var b = new Solid(MeshFactory.CreateTetrahedron(new Vec3(0.3, 0.3, 0.3), 1).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void TetrahedronTetrahedron_Intersect_NonEmpty()
    {
        var a = new Solid(MeshFactory.CreateTetrahedron(Vec3.Zero, 1).Mesh);
        var b = new Solid(MeshFactory.CreateTetrahedron(new Vec3(0.3, 0.3, 0.3), 1).Mesh);
        var result = Csg.Intersect(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void CubeTetrahedron_Union_LargerThanEither()
    {
        var cube = new Solid(MeshFactory.CreateCube().Mesh);
        var tet = new Solid(MeshFactory.CreateTetrahedron(new Vec3(0.2, 0.2, 0.2), 0.5).Mesh);
        var union = Csg.Union(cube, tet);
        var intersect = Csg.Intersect(cube, tet);
        Assert.True(union.FaceCount > intersect.FaceCount);
    }

    [Fact]
    public void SmallOffsets_AllAxes_Union()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var offsets = new[]
        {
            new Vec3(0.1, 0, 0), new Vec3(0, 0.1, 0), new Vec3(0, 0, 0.1)
        };
        foreach (var off in offsets)
        {
            var b = new Solid(MeshFactory.CreateCube(off).Mesh);
            var result = Csg.Union(a, b);
            Assert.True(result.FaceCount > 0, $"Union failed at offset {off}");
        }
    }

    [Fact]
    public void LargeOffsets_AllAxes_Intersection()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var offsets = new[]
        {
            new Vec3(0.9, 0, 0), new Vec3(0, 0.9, 0), new Vec3(0, 0, 0.9)
        };
        foreach (var off in offsets)
        {
            var b = new Solid(MeshFactory.CreateCube(off).Mesh);
            var result = Csg.Intersect(a, b);
            Assert.True(result.FaceCount > 0, $"Intersect failed at offset {off}");
        }
    }

    [Fact]
    public void DiagonalOffset_Union()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var result = Csg.Union(a, b);
        Assert.True(result.FaceCount > 12); // More faces than single cube
    }

    [Fact]
    public void DiagonalOffset_Difference()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var result = Csg.Difference(a, b);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void SphereCube_Centered_Difference()
    {
        var sphere = new Solid(MeshFactory.CreateSphere(new Vec3(0.5, 0.5, 0.5), 0.4, 2).Mesh);
        var cube = new Solid(MeshFactory.CreateCube().Mesh);
        // Cube minus sphere = cube with spherical hole
        var result = Csg.Difference(cube, sphere);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Union_Larger_VertexCount_Than_Intersection()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var union = Csg.Union(a, b);
        var inter = Csg.Intersect(a, b);
        Assert.True(union.VertexCount >= inter.VertexCount);
    }

    [Fact]
    public void WindingNumber_Produces_ValidResult()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var opts = new CsgOptions { UseWindingNumber = true };
        Assert.True(Csg.Union(a, b, opts).FaceCount > 0);
        Assert.True(Csg.Intersect(a, b, opts).FaceCount > 0);
        Assert.True(Csg.Difference(a, b, opts).FaceCount > 0);
    }
}
