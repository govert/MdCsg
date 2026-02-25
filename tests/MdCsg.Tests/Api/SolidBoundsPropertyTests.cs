using MdCsg.Api;
using MdCsg.Math;
using MdCsg.Tests.TestHelpers;

namespace MdCsg.Tests.Api;

/// <summary>Phase 6: Solid — factory methods, Bounds, Mesh, Bvh accessors</summary>
public class SolidBoundsPropertyTests
{
    [Fact]
    public void FromTriangles_SingleTriangle_HasOneFace()
    {
        var solid = Solid.FromTriangles(new[]
        {
            new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0))
        });
        Assert.Equal(1, solid.Mesh.Faces.Count);
    }

    [Fact]
    public void FromTriangles_Empty_HasNoFaces()
    {
        var solid = Solid.FromTriangles(Array.Empty<Triangle3>());
        Assert.Equal(0, solid.Mesh.Faces.Count);
    }

    [Fact]
    public void FromIndexed_TwoTriangles_HasTwoFaces()
    {
        var positions = new Vec3[]
        {
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0), new Vec3(1, 1, 0)
        };
        var indices = new (int, int, int)[] { (0, 1, 2), (1, 3, 2) };
        var solid = Solid.FromIndexed(positions, indices);
        Assert.Equal(2, solid.Mesh.Faces.Count);
    }

    [Fact]
    public void Solid_Mesh_NotNull()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        Assert.NotNull(cube.Mesh);
    }

    [Fact]
    public void Solid_Bvh_NotNull()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        Assert.NotNull(cube.Bvh);
    }

    [Fact]
    public void Solid_Bounds_EnclosesMesh()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var bounds = cube.Bounds;
        foreach (var v in cube.Mesh.Vertices)
        {
            Assert.True(v.Position.X >= bounds.Min.X - 1e-10);
            Assert.True(v.Position.Y >= bounds.Min.Y - 1e-10);
            Assert.True(v.Position.Z >= bounds.Min.Z - 1e-10);
            Assert.True(v.Position.X <= bounds.Max.X + 1e-10);
            Assert.True(v.Position.Y <= bounds.Max.Y + 1e-10);
            Assert.True(v.Position.Z <= bounds.Max.Z + 1e-10);
        }
    }

    [Fact]
    public void Solid_Bounds_CubeSize_Correct()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 5.0);
        var size = cube.Bounds.Size;
        Assert.True(System.Math.Abs(size.X - 5.0) < 0.01);
        Assert.True(System.Math.Abs(size.Y - 5.0) < 0.01);
        Assert.True(System.Math.Abs(size.Z - 5.0) < 0.01);
    }

    [Fact]
    public void Solid_Bounds_SphereRadius_Correct()
    {
        var sphere = MeshFactory.CreateSphere(Vec3.Zero, 3.0, 2);
        var bounds = sphere.Bounds;
        Assert.True(bounds.Max.X > 2.9 && bounds.Max.X < 3.1);
        Assert.True(bounds.Min.X > -3.1 && bounds.Min.X < -2.9);
    }

    [Fact]
    public void Solid_FromTriangles_CustomTolerance()
    {
        var solid = Solid.FromTriangles(new[]
        {
            new Triangle3(new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0))
        }, weldTolerance: 1e-6);
        Assert.Equal(1, solid.Mesh.Faces.Count);
    }

    [Fact]
    public void Solid_FromIndexed_CustomTolerance()
    {
        var positions = new Vec3[]
        {
            new Vec3(0, 0, 0), new Vec3(1, 0, 0), new Vec3(0, 1, 0)
        };
        var indices = new (int, int, int)[] { (0, 1, 2) };
        var solid = Solid.FromIndexed(positions, indices, weldTolerance: 1e-12);
        Assert.Equal(1, solid.Mesh.Faces.Count);
    }

    [Fact]
    public void Solid_Constructor_FromMesh()
    {
        var cube = MeshFactory.CreateCube(Vec3.Zero, 2.0);
        var solid = new Solid(cube.Mesh);
        Assert.Same(cube.Mesh, solid.Mesh);
        Assert.NotNull(solid.Bvh);
    }
}
