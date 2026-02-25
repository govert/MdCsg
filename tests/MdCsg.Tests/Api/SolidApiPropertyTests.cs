using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Api;

/// <summary>Phase 6: Solid API — construction, bounds, BVH, FromTriangles, FromIndexed</summary>
public class SolidApiPropertyTests
{
    [Fact]
    public void FromTriangles_CreatesValidSolid()
    {
        var triangles = new List<Triangle3>
        {
            new(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new(new Vec3(1, 0, 0), new Vec3(1, 1, 0), new Vec3(0, 1, 0)),
        };
        var solid = Solid.FromTriangles(triangles);
        Assert.NotNull(solid.Mesh);
        Assert.NotNull(solid.Bvh);
        Assert.Equal(2, solid.Mesh.Faces.Count);
    }

    [Fact]
    public void FromIndexed_CreatesValidSolid()
    {
        var positions = new Vec3[]
        {
            Vec3.Zero, new Vec3(1, 0, 0), new Vec3(1, 1, 0), new Vec3(0, 1, 0)
        };
        var triangles = new (int, int, int)[] { (0, 1, 2), (0, 2, 3) };
        var solid = Solid.FromIndexed(positions, triangles);
        Assert.NotNull(solid.Mesh);
        Assert.Equal(2, solid.Mesh.Faces.Count);
        Assert.Equal(4, solid.Mesh.Vertices.Count);
    }

    [Fact]
    public void Bounds_MatchesMeshBounds()
    {
        var solid = MeshFactory.CreateCube();
        var meshBounds = solid.Mesh.GetBounds();
        var solidBounds = solid.Bounds;
        Assert.Equal(meshBounds.Min, solidBounds.Min);
        Assert.Equal(meshBounds.Max, solidBounds.Max);
    }

    [Fact]
    public void Bvh_HasNodes()
    {
        var solid = MeshFactory.CreateCube();
        Assert.True(solid.Bvh.NodeCount > 0);
    }

    [Fact]
    public void Bvh_MeshReference()
    {
        var solid = MeshFactory.CreateCube();
        Assert.Same(solid.Mesh, solid.Bvh.Mesh);
    }

    [Fact]
    public void Cube_Bounds_IsUnitCube()
    {
        var solid = MeshFactory.CreateCube();
        Assert.Equal(0, solid.Bounds.Min.X, 1e-10);
        Assert.Equal(0, solid.Bounds.Min.Y, 1e-10);
        Assert.Equal(0, solid.Bounds.Min.Z, 1e-10);
        Assert.Equal(1, solid.Bounds.Max.X, 1e-10);
        Assert.Equal(1, solid.Bounds.Max.Y, 1e-10);
        Assert.Equal(1, solid.Bounds.Max.Z, 1e-10);
    }

    [Fact]
    public void Sphere_BvhNodeCount_Positive()
    {
        var solid = MeshFactory.CreateSphere(Vec3.Zero, 1.0, 2);
        Assert.True(solid.Bvh.NodeCount > 0);
    }

    [Fact]
    public void FromTriangles_WeldTolerance_MergesNearVertices()
    {
        var triangles = new List<Triangle3>
        {
            new(Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new(new Vec3(1e-11, 0, 0), new Vec3(1, 1e-11, 0), new Vec3(0, 1, 1e-11)),
        };
        var solid = Solid.FromTriangles(triangles, weldTolerance: 1e-8);
        // Near-coincident vertices should be welded
        Assert.True(solid.Mesh.Vertices.Count <= 4);
    }

    [Fact]
    public void Csg_Union_ReturnsCsgResult()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5));
        var result = Csg.Union(a, b);
        Assert.NotNull(result);
        Assert.NotNull(result.Mesh);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Csg_Intersect_ReturnsCsgResult()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5));
        var result = Csg.Intersect(a, b);
        Assert.NotNull(result);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void Csg_Difference_ReturnsCsgResult()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5));
        var result = Csg.Difference(a, b);
        Assert.NotNull(result);
        Assert.True(result.FaceCount > 0);
    }

    [Fact]
    public void CsgResult_PatchCounts_NonNegative()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5));
        var result = Csg.Union(a, b);
        Assert.True(result.PatchCountA >= 0);
        Assert.True(result.PatchCountB >= 0);
    }

    [Fact]
    public void CsgResult_IntersectionSegmentCount_Positive()
    {
        var a = MeshFactory.CreateCube();
        var b = MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5));
        var result = Csg.Union(a, b);
        Assert.True(result.IntersectionSegmentCount > 0);
    }

    [Fact]
    public void Solid_Tetrahedron_HasBvh()
    {
        var solid = MeshFactory.CreateTetrahedron();
        Assert.True(solid.Bvh.NodeCount > 0);
    }
}
