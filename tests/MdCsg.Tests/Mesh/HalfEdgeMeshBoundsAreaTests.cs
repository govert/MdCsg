using MdCsg.Math;
using MdCsg.Mesh;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Mesh;

/// <summary>Phase 6: HalfEdgeMesh GetBounds and surface area — bounds correctness, empty mesh, offset</summary>
public class HalfEdgeMeshBoundsAreaTests
{
    [Fact]
    public void GetBounds_UnitCube()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bounds = mesh.GetBounds();
        Assert.Equal(0, bounds.Min.X, 1e-10);
        Assert.Equal(0, bounds.Min.Y, 1e-10);
        Assert.Equal(0, bounds.Min.Z, 1e-10);
        Assert.Equal(1, bounds.Max.X, 1e-10);
        Assert.Equal(1, bounds.Max.Y, 1e-10);
        Assert.Equal(1, bounds.Max.Z, 1e-10);
    }

    [Fact]
    public void GetBounds_OffsetCube()
    {
        var offset = new Vec3(10, 20, 30);
        var mesh = MeshFactory.CreateCube(offset).Mesh;
        var bounds = mesh.GetBounds();
        Assert.Equal(10, bounds.Min.X, 1e-10);
        Assert.Equal(20, bounds.Min.Y, 1e-10);
        Assert.Equal(30, bounds.Min.Z, 1e-10);
        Assert.Equal(11, bounds.Max.X, 1e-10);
        Assert.Equal(21, bounds.Max.Y, 1e-10);
        Assert.Equal(31, bounds.Max.Z, 1e-10);
    }

    [Fact]
    public void GetBounds_ScaledCube()
    {
        var mesh = MeshFactory.CreateCube(Vec3.Zero, 3.0).Mesh;
        var bounds = mesh.GetBounds();
        Assert.Equal(0, bounds.Min.X, 1e-10);
        Assert.Equal(3, bounds.Max.X, 1e-10);
        Assert.Equal(3, bounds.Max.Y, 1e-10);
        Assert.Equal(3, bounds.Max.Z, 1e-10);
    }

    [Fact]
    public void GetBounds_EmptyMesh_IsEmpty()
    {
        var mesh = new MeshBuilder().Build(new List<Triangle3>());
        var bounds = mesh.GetBounds();
        Assert.True(bounds.Min.X > bounds.Max.X); // Aabb.Empty
    }

    [Fact]
    public void GetBounds_Sphere_ApproximatesRadius()
    {
        double radius = 2.0;
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, radius, 2).Mesh;
        var bounds = mesh.GetBounds();
        // Icosphere vertices are on the sphere, bounds should be close to [-r, r]
        Assert.True(bounds.Min.X >= -radius - 0.01 && bounds.Min.X <= -radius + 0.01);
        Assert.True(bounds.Max.X >= radius - 0.01 && bounds.Max.X <= radius + 0.01);
    }

    [Fact]
    public void GetBounds_ContainsAllVertices()
    {
        var mesh = MeshFactory.CreateSphere(new Vec3(1, 2, 3), 1.5, 2).Mesh;
        var bounds = mesh.GetBounds();
        foreach (var v in mesh.Vertices)
        {
            Assert.True(bounds.Contains(v.Position),
                $"Bounds should contain vertex {v.Position}");
        }
    }

    [Fact]
    public void SurfaceArea_UnitCube_Is6()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        double totalArea = 0;
        foreach (var face in mesh.Faces)
        {
            face.GetTrianglePositions(out var a, out var b, out var c);
            totalArea += new Triangle3(a, b, c).Area;
        }
        Assert.Equal(6.0, totalArea, 1e-10);
    }

    [Fact]
    public void SurfaceArea_ScaledCube()
    {
        var mesh = MeshFactory.CreateCube(Vec3.Zero, 2.0).Mesh;
        double totalArea = 0;
        foreach (var face in mesh.Faces)
        {
            face.GetTrianglePositions(out var a, out var b, out var c);
            totalArea += new Triangle3(a, b, c).Area;
        }
        Assert.Equal(24.0, totalArea, 1e-10); // 6 * 4
    }

    [Fact]
    public void SurfaceArea_Sphere_ApproximatesAnalytic()
    {
        double radius = 1.0;
        var mesh = MeshFactory.CreateSphere(Vec3.Zero, radius, 3).Mesh;
        double totalArea = 0;
        foreach (var face in mesh.Faces)
        {
            face.GetTrianglePositions(out var a, out var b, out var c);
            totalArea += new Triangle3(a, b, c).Area;
        }
        double expected = 4 * System.Math.PI * radius * radius;
        Assert.True(System.Math.Abs(totalArea - expected) / expected < 0.02,
            $"Surface area {totalArea} should approximate {expected} within 2%");
    }

    [Fact]
    public void GetBounds_Tetrahedron_ContainsAllVertices()
    {
        var mesh = MeshFactory.CreateTetrahedron().Mesh;
        var bounds = mesh.GetBounds();
        foreach (var v in mesh.Vertices)
        {
            Assert.True(bounds.Contains(v.Position));
        }
    }

    [Fact]
    public void GetBounds_CsgResult_ContainsAllVertices()
    {
        var a = new MdCsg.Api.Solid(MeshFactory.CreateCube().Mesh);
        var b = new MdCsg.Api.Solid(MeshFactory.CreateCube(new Vec3(0.5, 0.5, 0.5)).Mesh);
        var result = MdCsg.Api.Csg.Union(a, b);
        var bounds = result.Mesh.GetBounds();
        foreach (var v in result.Mesh.Vertices)
        {
            Assert.True(bounds.Contains(v.Position));
        }
    }

    [Fact]
    public void Bounds_Overlaps_SameShape()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var bounds = mesh.GetBounds();
        Assert.True(bounds.Overlaps(bounds));
    }

    [Fact]
    public void Bounds_Center_IsCorrect()
    {
        var mesh = MeshFactory.CreateCube().Mesh;
        var center = mesh.GetBounds().Center;
        Assert.Equal(0.5, center.X, 1e-10);
        Assert.Equal(0.5, center.Y, 1e-10);
        Assert.Equal(0.5, center.Z, 1e-10);
    }
}
