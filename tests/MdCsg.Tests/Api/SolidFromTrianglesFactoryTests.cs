using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Api;

/// <summary>Phase 6: Solid factory — FromTriangles, FromIndexed, Bounds, BVH consistency</summary>
public class SolidFromTrianglesFactoryTests
{
    [Fact]
    public void FromTriangles_SingleTriangle_HasOneFace()
    {
        var triangles = new[]
        {
            new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0))
        };
        var solid = Solid.FromTriangles(triangles);
        Assert.Equal(1, solid.Mesh.Faces.Count);
    }

    [Fact]
    public void FromTriangles_CubeTriangles_Has12Faces()
    {
        var cubeData = MeshFactory.CreateCube();
        var triangles = new List<Triangle3>();
        foreach (var face in cubeData.Mesh.Faces)
        {
            face.GetTrianglePositions(out var a, out var b, out var c);
            triangles.Add(new Triangle3(a, b, c));
        }
        var solid = Solid.FromTriangles(triangles);
        Assert.Equal(12, solid.Mesh.Faces.Count);
    }

    [Fact]
    public void FromTriangles_WeldsNearbyVertices()
    {
        var triangles = new[]
        {
            new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new Triangle3(new Vec3(1.0000000001, 0, 0), new Vec3(0, 0, 0), new Vec3(0, 0, 1)),
        };
        var solid = Solid.FromTriangles(triangles, weldTolerance: 1e-8);
        // Vertices at (1,0,0) and (1+1e-10,0,0) should be welded
        Assert.True(solid.Mesh.Vertices.Count <= 4);
    }

    [Fact]
    public void FromTriangles_NoWelding_KeepsAllVertices()
    {
        var triangles = new[]
        {
            new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)),
            new Triangle3(new Vec3(1, 0, 0), new Vec3(0, 0, 0), new Vec3(0, 0, 1)),
        };
        // With zero tolerance, no welding occurs
        var solid = Solid.FromTriangles(triangles, weldTolerance: 0);
        Assert.Equal(6, solid.Mesh.Vertices.Count);
    }

    [Fact]
    public void FromIndexed_TwoTriangles_SharedVertices()
    {
        var positions = new Vec3[]
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 1, 0), new(0, 0, 1)
        };
        var indices = new (int, int, int)[] { (0, 1, 2), (1, 0, 3) };
        var solid = Solid.FromIndexed(positions, indices);
        Assert.Equal(2, solid.Mesh.Faces.Count);
        Assert.Equal(4, solid.Mesh.Vertices.Count);
    }

    [Fact]
    public void FromIndexed_SharedIndices_ShareVertices()
    {
        // Using shared indices means fewer vertices
        var positions = new Vec3[]
        {
            new(0, 0, 0), new(1, 0, 0), new(0, 1, 0), new(0, 0, 1)
        };
        var indices = new (int, int, int)[] { (0, 1, 2), (0, 1, 3) };
        var solid = Solid.FromIndexed(positions, indices);
        // Indexed build uses position array as-is, all 4 vertices created
        Assert.Equal(4, solid.Mesh.Vertices.Count);
        Assert.Equal(2, solid.Mesh.Faces.Count);
    }

    [Fact]
    public void Solid_HasBvh()
    {
        var solid = new Solid(MeshFactory.CreateCube().Mesh);
        Assert.NotNull(solid.Bvh);
        Assert.True(solid.Bvh.NodeCount > 0);
    }

    [Fact]
    public void Solid_Bounds_ContainsAllVertices()
    {
        var solid = new Solid(MeshFactory.CreateCube().Mesh);
        var bounds = solid.Bounds;
        foreach (var v in solid.Mesh.Vertices)
        {
            Assert.True(bounds.Contains(v.Position),
                $"Vertex {v.Position} outside bounds {bounds}");
        }
    }

    [Fact]
    public void Solid_Bounds_CubeIsUnitSized()
    {
        var solid = new Solid(MeshFactory.CreateCube().Mesh);
        var bounds = solid.Bounds;
        var size = bounds.Size;
        Assert.True(System.Math.Abs(size.X - 1) < 1e-10);
        Assert.True(System.Math.Abs(size.Y - 1) < 1e-10);
        Assert.True(System.Math.Abs(size.Z - 1) < 1e-10);
    }

    [Fact]
    public void Solid_Bounds_OffsetCube()
    {
        var offset = new Vec3(5, 10, 15);
        var solid = new Solid(MeshFactory.CreateCube(offset).Mesh);
        var bounds = solid.Bounds;
        Assert.True(System.Math.Abs(bounds.Min.X - offset.X) < 1e-10);
        Assert.True(System.Math.Abs(bounds.Min.Y - offset.Y) < 1e-10);
        Assert.True(System.Math.Abs(bounds.Min.Z - offset.Z) < 1e-10);
    }

    [Fact]
    public void FromTriangles_EmptyList_EmptyMesh()
    {
        var solid = Solid.FromTriangles(Array.Empty<Triangle3>());
        Assert.Empty(solid.Mesh.Faces);
        Assert.Empty(solid.Mesh.Vertices);
    }

    [Fact]
    public void FromIndexed_EmptyIndices_EmptyMesh()
    {
        var positions = new Vec3[] { new(0, 0, 0) };
        var indices = Array.Empty<(int, int, int)>();
        var solid = Solid.FromIndexed(positions, indices);
        Assert.Empty(solid.Mesh.Faces);
    }

    [Fact]
    public void Solid_BvhAndMesh_SameFaceCount()
    {
        var solid = new Solid(MeshFactory.CreateCube().Mesh);
        // BVH should index all faces
        Assert.Equal(solid.Mesh.Faces.Count, solid.Bvh.Mesh.Faces.Count);
    }

    [Fact]
    public void Solid_Sphere_BoundsMatchRadius()
    {
        double r = 2.0;
        var solid = new Solid(MeshFactory.CreateSphere(Vec3.Zero, r, 3).Mesh);
        var bounds = solid.Bounds;
        // Sphere bounds should be approximately [-r, r] in all axes
        Assert.True(bounds.Max.X <= r + 0.01);
        Assert.True(bounds.Min.X >= -r - 0.01);
    }
}
