using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Api;

/// <summary>Phase 6: Solid — constructor, FromTriangles, FromIndexed, Bounds, Bvh properties</summary>
public class SolidFactoryPropertyTests
{
    [Fact]
    public void Constructor_FromMesh_PreservesMesh()
    {
        var cube = MeshFactory.CreateCube();
        var solid = new Solid(cube.Mesh);
        Assert.Same(cube.Mesh, solid.Mesh);
    }

    [Fact]
    public void Constructor_BuildsBvh()
    {
        var cube = MeshFactory.CreateCube();
        var solid = new Solid(cube.Mesh);
        Assert.NotNull(solid.Bvh);
        Assert.True(solid.Bvh.NodeCount > 0);
    }

    [Fact]
    public void FromTriangles_CreatesValidSolid()
    {
        var triangles = new Triangle3[]
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new(new Vec3(0, 0, 0), new Vec3(0, 1, 0), new Vec3(0, 0, 1)),
        };
        var solid = Solid.FromTriangles(triangles);
        Assert.Equal(2, solid.Mesh.Faces.Count);
    }

    [Fact]
    public void FromIndexed_CreatesValidSolid()
    {
        var positions = new Vec3[]
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 1, 0), new(0, 0, 1)
        };
        var indices = new (int, int, int)[]
        {
            (0, 1, 2),
            (0, 2, 3),
        };
        var solid = Solid.FromIndexed(positions, indices);
        Assert.Equal(2, solid.Mesh.Faces.Count);
    }

    [Fact]
    public void Bounds_Cube_CorrectRange()
    {
        var cube = MeshFactory.CreateCube();
        var bounds = cube.Bounds;
        Assert.True(bounds.Min.X >= -0.01);
        Assert.True(bounds.Max.X <= 1.01);
    }

    [Fact]
    public void Bounds_OffsetCube_Shifted()
    {
        var cube = MeshFactory.CreateCube(new Vec3(10, 20, 30));
        var bounds = cube.Bounds;
        Assert.True(bounds.Min.X >= 9.99);
        Assert.True(bounds.Max.X <= 11.01);
    }

    [Fact]
    public void Bounds_Sphere_ContainsCenter()
    {
        var sphere = MeshFactory.CreateSphere(new Vec3(5, 5, 5), 1.0, 2);
        Assert.True(sphere.Bounds.Contains(new Vec3(5, 5, 5)));
    }

    [Fact]
    public void Bounds_Sphere_ApproximateRadius()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 2.0, 2);
        var bounds = sphere.Bounds;
        // Icosphere vertices lie on the sphere, so bounds should be close to [-2,2]
        Assert.True(bounds.Max.X > 1.8);
        Assert.True(bounds.Min.X < -1.8);
    }

    [Fact]
    public void FromTriangles_WithWeldTolerance_MergesVertices()
    {
        // Two triangles sharing an edge — vertices should be welded
        var triangles = new Triangle3[]
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new(new Vec3(1, 0, 0), new Vec3(1, 1, 0), new Vec3(0, 1, 0)),
        };
        var solid = Solid.FromTriangles(triangles, 1e-8);
        Assert.Equal(2, solid.Mesh.Faces.Count);
        Assert.Equal(4, solid.Mesh.Vertices.Count); // 4 unique vertices, not 6
    }

    [Fact]
    public void Bvh_Mesh_MatchesSolidMesh()
    {
        var cube = MeshFactory.CreateCube();
        Assert.Same(cube.Mesh, cube.Bvh.Mesh);
    }

    [Fact]
    public void MeshFactory_CubeHas12Faces()
    {
        var cube = MeshFactory.CreateCube();
        Assert.Equal(12, cube.Mesh.Faces.Count);
    }

    [Fact]
    public void MeshFactory_CubeHas8Vertices()
    {
        var cube = MeshFactory.CreateCube();
        Assert.Equal(8, cube.Mesh.Vertices.Count);
    }

    [Fact]
    public void MeshFactory_TetrahedronHas4Faces()
    {
        var tet = MeshFactory.CreateTetrahedron();
        Assert.Equal(4, tet.Mesh.Faces.Count);
    }

    [Fact]
    public void MeshFactory_TetrahedronHas4Vertices()
    {
        var tet = MeshFactory.CreateTetrahedron();
        Assert.Equal(4, tet.Mesh.Vertices.Count);
    }
}
