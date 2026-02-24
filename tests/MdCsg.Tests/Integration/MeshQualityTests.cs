using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Integration;

/// <summary>Batch 33: Mesh quality and validation tests (20 tests)</summary>
public class MeshQualityTests
{
    [Fact]
    public void Cube_IsClosedManifold()
    {
        var cube = MeshFactory.CreateCube();
        MeshAssertions.AssertClosedManifold(cube.Mesh);
    }

    [Fact]
    public void Tetrahedron_IsClosedManifold()
    {
        var tet = MeshFactory.CreateTetrahedron();
        MeshAssertions.AssertClosedManifold(tet.Mesh);
    }

    [Fact]
    public void Sphere_IsClosedManifold()
    {
        var sphere = MeshFactory.CreateSphere(subdivisions: 2);
        MeshAssertions.AssertClosedManifold(sphere.Mesh);
    }

    [Fact]
    public void Cube_HasEulerCharacteristic2()
    {
        var cube = MeshFactory.CreateCube();
        var result = MeshValidator.Validate(cube.Mesh);
        Assert.Equal(2, result.EulerCharacteristic);
    }

    [Fact]
    public void Tetrahedron_HasEulerCharacteristic2()
    {
        var tet = MeshFactory.CreateTetrahedron();
        var result = MeshValidator.Validate(tet.Mesh);
        Assert.Equal(2, result.EulerCharacteristic);
    }

    [Fact]
    public void Sphere_HasEulerCharacteristic2()
    {
        var sphere = MeshFactory.CreateSphere(subdivisions: 2);
        var result = MeshValidator.Validate(sphere.Mesh);
        Assert.Equal(2, result.EulerCharacteristic);
    }

    [Fact]
    public void Cube_IsConsistentlyOriented()
    {
        var cube = MeshFactory.CreateCube();
        var result = MeshValidator.Validate(cube.Mesh);
        Assert.True(result.IsConsistentlyOriented);
    }

    [Fact]
    public void Cube_HasValidFaceCycles()
    {
        var cube = MeshFactory.CreateCube();
        var result = MeshValidator.Validate(cube.Mesh);
        Assert.True(result.HasValidFaceCycles);
    }

    [Fact]
    public void Union_Result_HasValidFaceCycles()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var result = Csg.Union(a, b);
        var validation = MeshValidator.Validate(result.Mesh);
        Assert.True(validation.HasValidFaceCycles);
    }

    [Fact]
    public void Intersect_Result_HasValidFaceCycles()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var result = Csg.Intersect(a, b);
        var validation = MeshValidator.Validate(result.Mesh);
        Assert.True(validation.HasValidFaceCycles);
    }

    [Fact]
    public void Difference_Result_HasValidFaceCycles()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(0.3, 0.3, 0.3)).Mesh);
        var result = Csg.Difference(a, b);
        var validation = MeshValidator.Validate(result.Mesh);
        Assert.True(validation.HasValidFaceCycles);
    }

    [Fact]
    public void Cube_12Faces()
    {
        var cube = MeshFactory.CreateCube();
        Assert.Equal(12, cube.Mesh.Faces.Count);
    }

    [Fact]
    public void Cube_8Vertices()
    {
        var cube = MeshFactory.CreateCube();
        Assert.Equal(8, cube.Mesh.Vertices.Count);
    }

    [Fact]
    public void Tetrahedron_4Faces()
    {
        var tet = MeshFactory.CreateTetrahedron();
        Assert.Equal(4, tet.Mesh.Faces.Count);
    }

    [Fact]
    public void Tetrahedron_4Vertices()
    {
        var tet = MeshFactory.CreateTetrahedron();
        Assert.Equal(4, tet.Mesh.Vertices.Count);
    }

    [Fact]
    public void DisjointUnion_24Faces()
    {
        var a = new Solid(MeshFactory.CreateCube().Mesh);
        var b = new Solid(MeshFactory.CreateCube(new Vec3(5, 0, 0)).Mesh);
        var result = Csg.Union(a, b);
        Assert.Equal(24, result.FaceCount);
    }

    [Fact]
    public void Cube_AllFaces_AreTriangles()
    {
        var cube = MeshFactory.CreateCube();
        foreach (var face in cube.Mesh.Faces)
        {
            face.GetTrianglePositions(out var a, out var b, out var c);
            var tri = new Triangle3(a, b, c);
            Assert.True(tri.Area > 0);
        }
    }

    [Fact]
    public void Cube_BoundsAreCorrect()
    {
        var cube = MeshFactory.CreateCube();
        var bounds = cube.Mesh.GetBounds();
        Assert.True(bounds.Min.X >= -0.01 && bounds.Min.X <= 0.01);
        Assert.True(bounds.Min.Y >= -0.01 && bounds.Min.Y <= 0.01);
        Assert.True(bounds.Min.Z >= -0.01 && bounds.Min.Z <= 0.01);
        Assert.True(bounds.Max.X >= 0.99 && bounds.Max.X <= 1.01);
        Assert.True(bounds.Max.Y >= 0.99 && bounds.Max.Y <= 1.01);
        Assert.True(bounds.Max.Z >= 0.99 && bounds.Max.Z <= 1.01);
    }

    [Fact]
    public void OffsetCube_BoundsAreCorrect()
    {
        var cube = MeshFactory.CreateCube(new Vec3(10, 20, 30));
        var bounds = cube.Mesh.GetBounds();
        Assert.True(bounds.Min.X >= 9.99 && bounds.Min.X <= 10.01);
        Assert.True(bounds.Min.Y >= 19.99 && bounds.Min.Y <= 20.01);
    }

    [Fact]
    public void Sphere_FaceCount_IncreasesWithSubdivisions()
    {
        var s1 = MeshFactory.CreateSphere(subdivisions: 1);
        var s2 = MeshFactory.CreateSphere(subdivisions: 2);
        Assert.True(s2.Mesh.Faces.Count > s1.Mesh.Faces.Count);
    }
}
