using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Api;

/// <summary>Phase 6: Solid bounds, Bvh, FromTriangles, FromIndexed — property verification</summary>
public class SolidBoundsPropertyTests
{
    [Fact]
    public void Bounds_UnitCube_ZeroToOne()
    {
        var solid = new Solid(MeshFactory.CreateCube().Mesh);
        var bounds = solid.Bounds;
        Assert.Equal(0, bounds.Min.X, 1e-10);
        Assert.Equal(0, bounds.Min.Y, 1e-10);
        Assert.Equal(0, bounds.Min.Z, 1e-10);
        Assert.Equal(1, bounds.Max.X, 1e-10);
        Assert.Equal(1, bounds.Max.Y, 1e-10);
        Assert.Equal(1, bounds.Max.Z, 1e-10);
    }

    [Fact]
    public void Bounds_OffsetCube()
    {
        var solid = new Solid(MeshFactory.CreateCube(new Vec3(5, 10, 15), 3).Mesh);
        var bounds = solid.Bounds;
        Assert.Equal(5, bounds.Min.X, 1e-10);
        Assert.Equal(10, bounds.Min.Y, 1e-10);
        Assert.Equal(15, bounds.Min.Z, 1e-10);
        Assert.Equal(8, bounds.Max.X, 1e-10);
        Assert.Equal(13, bounds.Max.Y, 1e-10);
        Assert.Equal(18, bounds.Max.Z, 1e-10);
    }

    [Fact]
    public void Bvh_NotNull()
    {
        var solid = new Solid(MeshFactory.CreateCube().Mesh);
        Assert.True(solid.Bvh.NodeCount > 0);
    }

    [Fact]
    public void Bvh_HasCorrectMesh()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var solid = new Solid(mesh);
        Assert.Equal(mesh, solid.Bvh.Mesh);
    }

    [Fact]
    public void Mesh_NotNull()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var solid = new Solid(mesh);
        Assert.Equal(mesh, solid.Mesh);
    }

    [Fact]
    public void FromTriangles_SingleTriangle()
    {
        var triangles = new List<Triangle3>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0))
        };
        var solid = Solid.FromTriangles(triangles);
        Assert.Equal(1, solid.Mesh.Faces.Count);
        Assert.Equal(3, solid.Mesh.Vertices.Count);
    }

    [Fact]
    public void FromTriangles_TwoSharedEdge()
    {
        var triangles = new List<Triangle3>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new(new Vec3(1, 0, 0), new Vec3(0, 0, 0), new Vec3(0, 0, 1)),
        };
        var solid = Solid.FromTriangles(triangles);
        Assert.Equal(2, solid.Mesh.Faces.Count);
        Assert.Equal(4, solid.Mesh.Vertices.Count);
    }

    [Fact]
    public void FromTriangles_CustomWeldTolerance()
    {
        var triangles = new List<Triangle3>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new(new Vec3(0.0001, 0, 0), new Vec3(1.0001, 0, 0), new Vec3(0.0001, 1, 0)),
        };
        // With large tolerance, vertices should weld
        var solid = Solid.FromTriangles(triangles, 0.01);
        Assert.True(solid.Mesh.Vertices.Count < 6);
    }

    [Fact]
    public void FromIndexed_SimpleQuad()
    {
        var positions = new List<Vec3>
        {
            new(0, 0, 0), new(1, 0, 0), new(1, 1, 0), new(0, 1, 0)
        };
        var indices = new List<(int, int, int)>
        {
            (0, 1, 2), (0, 2, 3)
        };
        var solid = Solid.FromIndexed(positions, indices);
        Assert.Equal(2, solid.Mesh.Faces.Count);
        Assert.Equal(4, solid.Mesh.Vertices.Count);
    }

    [Fact]
    public void FromIndexed_BoundsCorrect()
    {
        var positions = new List<Vec3>
        {
            new(0, 0, 0), new(2, 0, 0), new(2, 2, 0), new(0, 2, 0),
            new(0, 0, 2), new(2, 0, 2), new(2, 2, 2), new(0, 2, 2),
        };
        var indices = new List<(int, int, int)>
        {
            (0, 1, 2), (0, 2, 3),
            (4, 6, 5), (4, 7, 6),
        };
        var solid = Solid.FromIndexed(positions, indices);
        var bounds = solid.Bounds;
        Assert.Equal(0, bounds.Min.X, 1e-10);
        Assert.Equal(0, bounds.Min.Y, 1e-10);
        Assert.Equal(0, bounds.Min.Z, 1e-10);
        Assert.Equal(2, bounds.Max.X, 1e-10);
        Assert.Equal(2, bounds.Max.Y, 1e-10);
        Assert.Equal(2, bounds.Max.Z, 1e-10);
    }

    [Fact]
    public void Sphere_Bounds_ContainRadius()
    {
        double radius = 2.0;
        var solid = new Solid(MeshFactory.CreateSphere(Vec3.Zero, radius, 2).Mesh);
        var bounds = solid.Bounds;
        // Icosphere bounds should approximately contain the sphere
        Assert.True(bounds.Min.X <= -radius * 0.95);
        Assert.True(bounds.Max.X >= radius * 0.95);
        Assert.True(bounds.Min.Y <= -radius * 0.95);
        Assert.True(bounds.Max.Y >= radius * 0.95);
    }

    [Fact]
    public void Tetrahedron_BvhNodeCount_Positive()
    {
        var solid = new Solid(MeshFactory.CreateTetrahedron().Mesh);
        Assert.True(solid.Bvh.NodeCount > 0);
    }

    [Fact]
    public void FromTriangles_BuildsBvh()
    {
        var triangles = new List<Triangle3>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new(new Vec3(0, 0, 0), new Vec3(0, 1, 0), new Vec3(0, 0, 1)),
        };
        var solid = Solid.FromTriangles(triangles);
        Assert.True(solid.Bvh.NodeCount > 0);
    }

    [Fact]
    public void Bounds_IsConsistentWithMesh()
    {
        var mesh = MeshFactory.CreateCube(new Vec3(3, 4, 5), 2).Mesh;
        var solid = new Solid(mesh);
        Assert.Equal(mesh.GetBounds().Min, solid.Bounds.Min);
        Assert.Equal(mesh.GetBounds().Max, solid.Bounds.Max);
    }
}
