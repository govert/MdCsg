using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Api;

/// <summary>Phase 6: Solid class construction tests — from mesh, from triangles, from indexed, bounds</summary>
public class SolidConstructionTests
{
    [Fact]
    public void FromMesh_Cube_HasBvh()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var solid = new Solid(mesh);
        Assert.Same(mesh, solid.Mesh);
        Assert.NotNull(solid.Bvh);
        Assert.True(solid.Bvh.NodeCount > 0);
    }

    [Fact]
    public void FromMesh_Sphere_HasBvh()
    {
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh;
        var solid = new Solid(mesh);
        Assert.Same(mesh, solid.Mesh);
        Assert.True(solid.Bvh.NodeCount > 0);
    }

    [Fact]
    public void FromTriangles_SingleTriangle()
    {
        var tris = new List<Triangle3>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0))
        };
        var solid = Solid.FromTriangles(tris);
        Assert.Equal(1, solid.Mesh.Faces.Count);
        Assert.Equal(3, solid.Mesh.Vertices.Count);
    }

    [Fact]
    public void FromTriangles_Tetrahedron()
    {
        var h = System.Math.Sqrt(2.0 / 3.0);
        var tris = new List<Triangle3>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0.5, System.Math.Sqrt(3) / 2, 0)),
            new(new Vec3(0, 0, 0), new Vec3(0.5, System.Math.Sqrt(3) / 2, 0), new Vec3(0.5, System.Math.Sqrt(3) / 6, h)),
            new(new Vec3(1, 0, 0), new Vec3(0.5, System.Math.Sqrt(3) / 6, h), new Vec3(0.5, System.Math.Sqrt(3) / 2, 0)),
            new(new Vec3(0, 0, 0), new Vec3(0.5, System.Math.Sqrt(3) / 6, h), new Vec3(1, 0, 0))
        };
        var solid = Solid.FromTriangles(tris);
        Assert.Equal(4, solid.Mesh.Faces.Count);
    }

    [Fact]
    public void FromIndexed_SingleTriangle()
    {
        var positions = new List<Vec3>
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 1, 0)
        };
        var indices = new List<(int, int, int)> { (0, 1, 2) };
        var solid = Solid.FromIndexed(positions, indices);
        Assert.Equal(1, solid.Mesh.Faces.Count);
        Assert.Equal(3, solid.Mesh.Vertices.Count);
    }

    [Fact]
    public void FromIndexed_TwoTriangles_SharedEdge()
    {
        var positions = new List<Vec3>
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 1, 0), new(1, 1, 0)
        };
        var indices = new List<(int, int, int)> { (0, 1, 2), (1, 3, 2) };
        var solid = Solid.FromIndexed(positions, indices);
        Assert.Equal(2, solid.Mesh.Faces.Count);
        Assert.Equal(4, solid.Mesh.Vertices.Count);
    }

    [Fact]
    public void Bounds_Cube_CorrectExtent()
    {
        var solid = new Solid(MeshFactory.CreateCube().Mesh);
        var bounds = solid.Bounds;
        Assert.True(bounds.Min.X >= -0.01);
        Assert.True(bounds.Max.X <= 1.01);
        Assert.True(bounds.Min.Y >= -0.01);
        Assert.True(bounds.Max.Y <= 1.01);
    }

    [Fact]
    public void Bounds_OffsetCube_CorrectExtent()
    {
        var solid = new Solid(MeshFactory.CreateCube(new Vec3(10, 20, 30)).Mesh);
        var bounds = solid.Bounds;
        Assert.True(bounds.Min.X >= 9.99);
        Assert.True(bounds.Max.X <= 11.01);
    }

    [Fact]
    public void Bounds_Sphere_ContainsAllVertices()
    {
        var solid = new Solid(MeshFactory.CreateSphere(Vec3.Zero, 1, 2).Mesh);
        var bounds = solid.Bounds;
        foreach (var v in solid.Mesh.Vertices)
        {
            Assert.True(bounds.Contains(v.Position));
        }
    }

    [Fact]
    public void FromTriangles_CustomWeldTolerance()
    {
        var tris = new List<Triangle3>
        {
            new(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0))
        };
        var solid = Solid.FromTriangles(tris, weldTolerance: 1e-6);
        Assert.Equal(1, solid.Mesh.Faces.Count);
    }
}
