using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Api;

/// <summary>Phase 6: Solid — FromTriangles, FromIndexed, Bounds, BVH validation</summary>
public class SolidFromIndexedTests
{
    private static List<Triangle3> GetCubeTriangles()
    {
        // Build the 12 triangles for a unit cube manually
        var v = new[]
        {
            new Vec3(0,0,0), new Vec3(1,0,0), new Vec3(1,1,0), new Vec3(0,1,0),
            new Vec3(0,0,1), new Vec3(1,0,1), new Vec3(1,1,1), new Vec3(0,1,1),
        };
        return new List<Triangle3>
        {
            new(v[0], v[1], v[2]), new(v[0], v[2], v[3]), // front
            new(v[5], v[4], v[7]), new(v[5], v[7], v[6]), // back
            new(v[3], v[2], v[6]), new(v[3], v[6], v[7]), // top
            new(v[4], v[5], v[1]), new(v[4], v[1], v[0]), // bottom
            new(v[1], v[5], v[6]), new(v[1], v[6], v[2]), // right
            new(v[4], v[0], v[3]), new(v[4], v[3], v[7]), // left
        };
    }

    [Fact]
    public void FromTriangles_CreatesCube()
    {
        var triangles = GetCubeTriangles();
        var solid = Solid.FromTriangles(triangles);
        Assert.NotNull(solid.Mesh);
        Assert.NotNull(solid.Bvh);
        Assert.Equal(12, solid.Mesh.Faces.Count);
    }

    [Fact]
    public void FromIndexed_CreatesCube()
    {
        var positions = new List<Vec3>
        {
            new(0, 0, 0), new(1, 0, 0), new(1, 1, 0), new(0, 1, 0),
            new(0, 0, 1), new(1, 0, 1), new(1, 1, 1), new(0, 1, 1),
        };
        var triangles = new List<(int, int, int)>
        {
            (0, 1, 2), (0, 2, 3),
            (5, 4, 7), (5, 7, 6),
            (3, 2, 6), (3, 6, 7),
            (4, 5, 1), (4, 1, 0),
            (1, 5, 6), (1, 6, 2),
            (4, 0, 3), (4, 3, 7),
        };
        var solid = Solid.FromIndexed(positions, triangles);
        Assert.Equal(12, solid.Mesh.Faces.Count);
        Assert.Equal(8, solid.Mesh.Vertices.Count);
    }

    [Fact]
    public void FromTriangles_Bounds_CorrectForCube()
    {
        var solid = Solid.FromTriangles(GetCubeTriangles());
        var bounds = solid.Bounds;
        Assert.True(bounds.Min.X <= 0.01 && bounds.Min.Y <= 0.01 && bounds.Min.Z <= 0.01);
        Assert.True(bounds.Max.X >= 0.99 && bounds.Max.Y >= 0.99 && bounds.Max.Z >= 0.99);
    }

    [Fact]
    public void FromTriangles_Bvh_HasNodes()
    {
        var solid = Solid.FromTriangles(GetCubeTriangles());
        Assert.True(solid.Bvh.NodeCount > 0);
    }

    [Fact]
    public void FromIndexed_Tetrahedron()
    {
        var positions = new List<Vec3>
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 1, 0), new(0, 0, 1),
        };
        var triangles = new List<(int, int, int)>
        {
            (0, 2, 1), (0, 1, 3), (0, 3, 2), (1, 2, 3),
        };
        var solid = Solid.FromIndexed(positions, triangles);
        Assert.Equal(4, solid.Mesh.Faces.Count);
    }

    [Fact]
    public void FromTriangles_Mesh_FacesAroundVertex()
    {
        var solid = Solid.FromTriangles(GetCubeTriangles());
        var v = solid.Mesh.Vertices[0];
        var faces = solid.Mesh.FacesAroundVertex(v).ToList();
        Assert.True(faces.Count > 0);
    }

    [Fact]
    public void Solid_BoundsMatchesMeshBounds()
    {
        var solid = Solid.FromTriangles(GetCubeTriangles());
        var solidBounds = solid.Bounds;
        var meshBounds = solid.Mesh.GetBounds();
        Assert.Equal(solidBounds.Min.X, meshBounds.Min.X, 10);
        Assert.Equal(solidBounds.Max.X, meshBounds.Max.X, 10);
    }

    [Fact]
    public void Solid_FromMeshFactory_HasBvh()
    {
        var factory = MeshFactory.CreateCube(new Vec3(5, 5, 5));
        Assert.NotNull(factory.Bvh);
        Assert.True(factory.Bvh.NodeCount > 0);
    }

    [Fact]
    public void Solid_Bounds_TranslatedCube()
    {
        var solid = MeshFactory.CreateCube(new Vec3(5, 5, 5));
        var bounds = solid.Bounds;
        Assert.True(bounds.Min.X >= 4.99);
        Assert.True(bounds.Max.X <= 6.01);
    }

    [Fact]
    public void FromIndexed_SingleTriangle()
    {
        var positions = new List<Vec3>
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 1, 0),
        };
        var triangles = new List<(int, int, int)> { (0, 1, 2) };
        var solid = Solid.FromIndexed(positions, triangles);
        Assert.Single(solid.Mesh.Faces);
        Assert.Equal(3, solid.Mesh.Vertices.Count);
    }
}
