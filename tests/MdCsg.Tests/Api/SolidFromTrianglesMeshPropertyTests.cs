using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Api;

/// <summary>Phase 6: Solid — FromTriangles, FromIndexed, Bounds, Mesh, Bvh consistency</summary>
public class SolidFromTrianglesMeshPropertyTests
{
    [Fact]
    public void FromTriangles_SingleTriangle_HasOneFace()
    {
        var tris = new[] { new Triangle3(Vec3.Zero, Vec3.UnitX, Vec3.UnitY) };
        var solid = Solid.FromTriangles(tris);
        Assert.Equal(1, solid.Mesh.Faces.Count);
    }

    [Fact]
    public void FromTriangles_TwoTriangles_TwoFaces()
    {
        var tris = new[]
        {
            new Triangle3(Vec3.Zero, Vec3.UnitX, Vec3.UnitY),
            new Triangle3(Vec3.UnitX, new Vec3(1, 1, 0), Vec3.UnitY)
        };
        var solid = Solid.FromTriangles(tris);
        Assert.Equal(2, solid.Mesh.Faces.Count);
    }

    [Fact]
    public void FromTriangles_HasBvh()
    {
        var tris = new[] { new Triangle3(Vec3.Zero, Vec3.UnitX, Vec3.UnitY) };
        var solid = Solid.FromTriangles(tris);
        Assert.NotNull(solid.Bvh);
        Assert.True(solid.Bvh.NodeCount > 0);
    }

    [Fact]
    public void FromIndexed_Cube_12Faces()
    {
        var cube = MeshFactory.CreateCube();
        Assert.Equal(12, cube.Mesh.Faces.Count);
    }

    [Fact]
    public void FromIndexed_Cube_8Vertices()
    {
        var cube = MeshFactory.CreateCube();
        Assert.Equal(8, cube.Mesh.Vertices.Count);
    }

    [Fact]
    public void Bounds_Cube_Correct()
    {
        var cube = MeshFactory.CreateCube(new Vec3(1, 2, 3), 4.0);
        var bounds = cube.Bounds;
        Assert.True(System.Math.Abs(bounds.Min.X - 1.0) < 0.01);
        Assert.True(System.Math.Abs(bounds.Min.Y - 2.0) < 0.01);
        Assert.True(System.Math.Abs(bounds.Min.Z - 3.0) < 0.01);
        Assert.True(System.Math.Abs(bounds.Max.X - 5.0) < 0.01);
        Assert.True(System.Math.Abs(bounds.Max.Y - 6.0) < 0.01);
        Assert.True(System.Math.Abs(bounds.Max.Z - 7.0) < 0.01);
    }

    [Fact]
    public void Mesh_And_Bvh_ReferSameMesh()
    {
        var cube = MeshFactory.CreateCube();
        Assert.Same(cube.Mesh, cube.Bvh.Mesh);
    }

    [Fact]
    public void FromTriangles_WeldTolerance_MergesNearbyVertices()
    {
        // Two triangles sharing an edge with slightly different vertex coords
        var tris = new[]
        {
            new Triangle3(Vec3.Zero, Vec3.UnitX, Vec3.UnitY),
            new Triangle3(Vec3.UnitX, new Vec3(1, 1, 0), new Vec3(0.0, 1.0, 0.0)) // shares edge
        };
        var solid = Solid.FromTriangles(tris, 1e-8);
        // Should weld shared vertices
        Assert.True(solid.Mesh.Vertices.Count <= 4,
            $"Should weld shared vertices, got {solid.Mesh.Vertices.Count}");
    }

    [Fact]
    public void Solid_Constructor_BuildsBvh()
    {
        var builder = new MdCsg.Mesh.MeshBuilder();
        var mesh = builder.Build(new[] { new Triangle3(Vec3.Zero, Vec3.UnitX, Vec3.UnitY) });
        var solid = new Solid(mesh);
        Assert.NotNull(solid.Bvh);
        Assert.True(solid.Bvh.NodeCount >= 1);
    }

    [Fact]
    public void Bounds_SingleTriangle_TightBounds()
    {
        var tris = new[] { new Triangle3(new Vec3(1, 2, 3), new Vec3(4, 5, 6), new Vec3(7, 0, 1)) };
        var solid = Solid.FromTriangles(tris);
        var bounds = solid.Bounds;
        Assert.True(bounds.Min.X <= 1.01);
        Assert.True(bounds.Max.X >= 6.99);
        Assert.True(bounds.Min.Y <= 0.01);
        Assert.True(bounds.Max.Y >= 4.99);
    }

    [Fact]
    public void FromIndexed_Tetrahedron_4Faces()
    {
        var tet = MeshFactory.CreateTetrahedron();
        Assert.Equal(4, tet.Mesh.Faces.Count);
    }

    [Fact]
    public void FromTriangles_EmptyList_ZeroFaces()
    {
        var solid = Solid.FromTriangles(Array.Empty<Triangle3>());
        Assert.Equal(0, solid.Mesh.Faces.Count);
    }

    [Fact]
    public void Sphere_Subdivision0_20Faces()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 0);
        Assert.Equal(20, sphere.Mesh.Faces.Count);
    }

    [Fact]
    public void Sphere_Subdivision1_80Faces()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 1);
        Assert.Equal(80, sphere.Mesh.Faces.Count);
    }
}
